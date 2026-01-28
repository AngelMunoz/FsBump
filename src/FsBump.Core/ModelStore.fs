namespace FsBump.Core

open System.Collections.Generic
open System.IO
open System.Globalization
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Mibo.Elmish
open Mibo.Rendering.Graphics3D

module ModelStore =
  open System

  [<Literal>]
  let PlayerBall = "kaykit_platformer/blue/ball_blue"

  [<Literal>]
  let Cube = "cube"

  type BakedShape = {
    Min: Vector3
    Max: Vector3
    Vertices: Vector3[]
    Indices: int[]
  }

  module Geometry =
    let extract(model: Model) =
      let mutable totalVertices = 0
      let mutable totalIndices = 0

      for mesh in model.Meshes do
        for part in mesh.MeshParts do
          totalVertices <- totalVertices + part.NumVertices
          totalIndices <- totalIndices + (part.PrimitiveCount * 3)

      let allVertices = Array.zeroCreate<Vector3> totalVertices
      let allIndices = Array.zeroCreate<int> totalIndices

      let mutable vertexOffset = 0
      let mutable indexOffset = 0

      let mutable min = Vector3(Single.MaxValue)
      let mutable max = Vector3(Single.MinValue)

      for mesh in model.Meshes do
        for part in mesh.MeshParts do
          let declaration = part.VertexBuffer.VertexDeclaration
          let elements = declaration.GetVertexElements()

          let posElement =
            elements
            |> Array.tryFind(fun e ->
              e.VertexElementUsage = VertexElementUsage.Position)

          match posElement with
          | Some elem ->
            part.VertexBuffer.GetData(
              part.VertexOffset * declaration.VertexStride + int elem.Offset,
              allVertices,
              vertexOffset,
              part.NumVertices,
              declaration.VertexStride
            )

            for i in 0 .. part.NumVertices - 1 do
              let v = allVertices.[vertexOffset + i]
              min <- Vector3.Min(min, v)
              max <- Vector3.Max(max, v)

            if
              part.IndexBuffer.IndexElementSize = IndexElementSize.SixteenBits
            then
              let partIndices =
                Array.zeroCreate<uint16>(part.PrimitiveCount * 3)

              part.IndexBuffer.GetData(
                part.StartIndex * 2,
                partIndices,
                0,
                part.PrimitiveCount * 3
              )

              for i in 0 .. partIndices.Length - 1 do
                allIndices.[indexOffset + i] <-
                  int partIndices.[i] + vertexOffset
            else
              let partIndices = Array.zeroCreate<int>(part.PrimitiveCount * 3)

              part.IndexBuffer.GetData(
                part.StartIndex * 4,
                partIndices,
                0,
                part.PrimitiveCount * 3
              )

              for i in 0 .. partIndices.Length - 1 do
                allIndices.[indexOffset + i] <- partIndices.[i] + vertexOffset

            vertexOffset <- vertexOffset + part.NumVertices
            indexOffset <- indexOffset + (part.PrimitiveCount * 3)
          | None -> ()

      BoundingBox(min, max),
      {
        Vertices = allVertices
        Indices = allIndices
      }

  module Serialization =
    let private inv = CultureInfo.InvariantCulture

    let private vecToStr(v: Vector3) =
      v.X.ToString("F3", inv)
      + ","
      + v.Y.ToString("F3", inv)
      + ","
      + v.Z.ToString("F3", inv)

    let serialize(shapes: seq<string * (ModelGeometry * BoundingBox)>) =
      let sb = System.Text.StringBuilder()

      for name, (geo, bounds) in shapes do
        let minS = vecToStr bounds.Min
        let maxS = vecToStr bounds.Max

        let vS = geo.Vertices |> Array.map vecToStr |> String.concat ","

        let iS = geo.Indices |> Array.map string |> String.concat ","

        sb.AppendLine(sprintf "%s|%s|%s|%s|%s" name minS maxS vS iS) |> ignore

      sb.ToString()

    let private parseVector(span: ReadOnlySpan<char>) =
      let mutable start = 0
      let coords = Array.zeroCreate<float32> 3
      let mutable count = 0

      for i in 0 .. span.Length - 1 do
        if span.[i] = ',' then
          coords.[count] <- Single.Parse(span.Slice(start, i - start), inv)
          start <- i + 1
          count <- count + 1

      coords.[2] <- Single.Parse(span.Slice(start), inv)
      Vector3(coords.[0], coords.[1], coords.[2])

    let deserialize(reader: StreamReader) =
      let map = Dictionary<string, BakedShape>()

      while not reader.EndOfStream do
        let line = reader.ReadLine()

        if not(String.IsNullOrWhiteSpace line) then
          let parts = line.Split('|')

          if parts.Length = 5 then
            let name = parts.[0]
            let min = parseVector(parts.[1].AsSpan())
            let max = parseVector(parts.[2].AsSpan())

            let vParts = parts.[3].Split(',')
            let vertices = Array.zeroCreate<Vector3>(vParts.Length / 3)

            for i in 0 .. vertices.Length - 1 do
              let x = Single.Parse(vParts.[i * 3], inv)
              let y = Single.Parse(vParts.[i * 3 + 1], inv)
              let z = Single.Parse(vParts.[i * 3 + 2], inv)
              vertices.[i] <- Vector3(x, y, z)

            let iParts = parts.[4].Split(',')

            let indices =
              Array.init iParts.Length (fun i -> Int32.Parse(iParts.[i], inv))

            map.[name] <- {
              Min = min
              Max = max
              Vertices = vertices
              Indices = indices
            }

      map

  module Persistence =
    let save ctx (content: string) =
      let path = "src/FsBump.Core/Content/collision.txt"
      File.WriteAllText(path, content)
      path

    let load ctx =
      Assets.fromCustom
        "collision.txt"
        (fun _ ->
          use str = TitleContainer.OpenStream "Content/collision.txt"
          use reader = new StreamReader(str)
          Serialization.deserialize reader)
        ctx

  let create(ctx: GameContext) =
    let modelCache = Dictionary<string, Model>()
    let meshCache = Dictionary<string, Mesh>()
    let boundsCache = Dictionary<string, BoundingBox>()
    let geometryCache = Dictionary<string, ModelGeometry>()
    let textureCache = Dictionary<string, Texture2D>()

    let getSpecificPath =
      function
      | Specific.PlayerBall -> PlayerBall
      | Specific.Cube -> Cube

    let populateCaches() =
      try
        let shapes = Persistence.load ctx

        for KeyValue(name, s) in shapes do
          boundsCache.[name] <- BoundingBox(s.Min, s.Max)

          geometryCache.[name] <- {
            Vertices = s.Vertices
            Indices = s.Indices
          }

        if shapes.Count > 0 then
          printfn "Loaded %d unique shapes from collision.txt" shapes.Count
      with ex ->
        printfn "Failed to load baked geometry: %s" ex.Message

    populateCaches()

    { new IModelStore with
        member _.Bake() =
          let shapesStr =
            modelCache
            |> Seq.map(fun kv -> kv.Key, kv.Value)
            |> Seq.distinctBy fst
            |> Seq.map(fun (key, model) ->
              let bounds, geo = Geometry.extract model
              key, (geo, bounds))
            |> Serialization.serialize

          let path = Persistence.save ctx shapesStr
          printfn "Baking complete. Saved to %s" path

        member _.Load(asset: AssetDefinition) =
          let assetName = AssetDefinition.getLoadPath asset

          if not(modelCache.ContainsKey assetName) then
            try
              let model = Assets.model assetName ctx
              modelCache[assetName] <- model

              if not(geometryCache.ContainsKey assetName) then
                let bounds, geometry = Geometry.extract model
                boundsCache[assetName] <- bounds
                geometryCache[assetName] <- geometry

              match Mesh.fromModel model |> Seq.tryHead with
              | Some mesh -> meshCache[assetName] <- mesh
              | None -> ()
            with ex ->
              printfn "Failed to load asset '%s': %O" asset.Name ex

        member _.LoadSpecific(specific: Specific) =
          let path = getSpecificPath specific

          try
            let model = Assets.model path ctx
            modelCache[path] <- model

            if not(geometryCache.ContainsKey path) then
              let bounds, geometry = Geometry.extract model
              boundsCache[path] <- bounds
              geometryCache[path] <- geometry

            if not(meshCache.ContainsKey path) then
              match Mesh.fromModel model |> Seq.tryHead with
              | Some mesh -> meshCache[path] <- mesh
              | None -> ()
          with ex ->
            printfn "Failed to load specific asset '%A': %O" specific ex


        member _.Get(asset: AssetDefinition) =
          let assetName = AssetDefinition.getLoadPath asset

          match modelCache.TryGetValue assetName with
          | true, v -> ValueSome v
          | _ -> ValueNone

        member _.GetMesh(asset: AssetDefinition) =
          let assetName = AssetDefinition.getLoadPath asset

          match meshCache.TryGetValue assetName with
          | true, v -> ValueSome v
          | _ -> ValueNone

        member _.GetBounds(asset: AssetDefinition) =
          let assetName = AssetDefinition.getLoadPath asset

          match boundsCache.TryGetValue(assetName) with
          | true, v -> ValueSome v
          | _ -> ValueNone

        member _.GetGeometry(asset: AssetDefinition) =
          let assetName = AssetDefinition.getLoadPath asset

          match geometryCache.TryGetValue(assetName) with
          | true, v -> ValueSome v
          | _ -> ValueNone

        member _.LoadTexture(name) =
          if not(textureCache.ContainsKey name) then
            try
              textureCache.[name] <- Assets.texture name ctx
            with _ ->
              ()

        member _.GetTexture(name) =
          match textureCache.TryGetValue name with
          | true, v -> ValueSome v
          | _ -> ValueNone

        member _.GetSpecificGeometry
          (specific: Specific)
          : ModelGeometry voption =
          let path = getSpecificPath specific

          match geometryCache.TryGetValue path with
          | true, v -> ValueSome v
          | _ -> ValueNone

        member _.GetSpecificMesh(specific: Specific) : Mesh voption =
          let path = getSpecificPath specific

          match meshCache.TryGetValue path with
          | true, v -> ValueSome v
          | _ -> ValueNone

        member _.GetSpecificBounds(arg: Specific) : BoundingBox voption =
          let path = getSpecificPath arg

          match boundsCache.TryGetValue path with
          | true, v -> ValueSome v
          | _ -> ValueNone

        member _.GetSpecific(specific) : Model voption =
          let path = getSpecificPath specific

          match modelCache.TryGetValue path with
          | true, v -> ValueSome v
          | _ -> ValueNone
    }
