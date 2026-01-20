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

  type BakedShape = {
    Min: Vector3
    Max: Vector3
    Vertices: Vector3[]
    Indices: int[]
  }

  let private extractModelData(model: Model) =
    let mutable min = Vector3(Single.MaxValue)
    let mutable max = Vector3(Single.MinValue)
    let allVertices = ResizeArray<Vector3>()
    let allIndices = ResizeArray<int>()
    let mutable vertexOffset = 0

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
          let partVertices = Array.zeroCreate<Vector3> part.NumVertices

          part.VertexBuffer.GetData(
            part.VertexOffset * declaration.VertexStride + int elem.Offset,
            partVertices,
            0,
            part.NumVertices,
            declaration.VertexStride
          )

          for v in partVertices do
            min <- Vector3.Min(min, v)
            max <- Vector3.Max(max, v)
            allVertices.Add(v)

          if
            part.IndexBuffer.IndexElementSize = IndexElementSize.SixteenBits
          then
            let partIndices = Array.zeroCreate<uint16>(part.PrimitiveCount * 3)

            part.IndexBuffer.GetData(
              part.StartIndex * 2,
              partIndices,
              0,
              part.PrimitiveCount * 3
            )

            for i in partIndices do
              allIndices.Add(int i + vertexOffset)
          else
            let partIndices = Array.zeroCreate<int>(part.PrimitiveCount * 3)

            part.IndexBuffer.GetData(
              part.StartIndex * 4,
              partIndices,
              0,
              part.PrimitiveCount * 3
            )

            for i in partIndices do
              allIndices.Add(i + vertexOffset)

          vertexOffset <- vertexOffset + part.NumVertices
        | None -> ()

    BoundingBox(min, max),
    {
      Vertices = allVertices.ToArray()
      Indices = allIndices.ToArray()
    }

  let getShapeKey(name: string) =
    if name.StartsWith("kaykit_platformer/") then
      let parts = name.Split('/')

      if parts.Length >= 3 then
        let baseName = parts.[2]

        if baseName.Contains("_") then
          baseName.Substring(0, baseName.LastIndexOf('_'))
        else
          baseName
      else
        name
    else
      name

  let create(ctx: GameContext) =
    let modelCache = Dictionary<string, Model>()
    let meshCache = Dictionary<string, Mesh>()
    let boundsCache = Dictionary<string, BoundingBox>()
    let geometryCache = Dictionary<string, ModelGeometry>()
    let textureCache = Dictionary<string, Texture2D>()

    let loadBaking() =
      try
        // Using Assets.fromCustom to load our custom collision.txt
        let shapes =
          Assets.fromCustom
            "collision.txt"
            (fun path ->
              use str = TitleContainer.OpenStream($"Content/{path}")
              use reader = new StreamReader(str)

              let lines =
                reader
                  .ReadToEnd()
                  .Split(
                    [| '\n'; '\r' |],
                    StringSplitOptions.RemoveEmptyEntries
                  )

              let map = Dictionary<string, BakedShape>()

              for line in lines do
                if not(String.IsNullOrWhiteSpace line) then
                  let parts = line.Split('|')

                  if parts.Length = 5 then
                    let name = parts.[0]

                    let minP =
                      parts.[1].Split(',')
                      |> Array.map(fun s -> float32(float s))

                    let maxP =
                      parts.[2].Split(',')
                      |> Array.map(fun s -> float32(float s))

                    let vP =
                      parts.[3].Split(',')
                      |> Array.map(fun s -> float32(float s))

                    let iP = parts.[4].Split(',') |> Array.map int

                    let vertices = Array.zeroCreate<Vector3>(vP.Length / 3)

                    for i in 0 .. vertices.Length - 1 do
                      vertices.[i] <-
                        Vector3(vP.[i * 3], vP.[i * 3 + 1], vP.[i * 3 + 2])

                    map.[name] <- {
                      Min = Vector3(minP.[0], minP.[1], minP.[2])
                      Max = Vector3(maxP.[0], maxP.[1], maxP.[2])
                      Vertices = vertices
                      Indices = iP
                    }

              map)
            ctx

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

    loadBaking()

    { new IModelStore with
        member this.Bake() =
          let shapes = Dictionary<string, ModelGeometry * BoundingBox>()

          for KeyValue(name, model) in modelCache do
            let key = getShapeKey name

            if not(shapes.ContainsKey key) then
              let bounds, geo = extractModelData model
              shapes.[key] <- (geo, bounds)

          let sb = System.Text.StringBuilder()

          for KeyValue(name, (geo, bounds)) in shapes do
            let minS =
              sprintf "%.3f,%.3f,%.3f" bounds.Min.X bounds.Min.Y bounds.Min.Z

            let maxS =
              sprintf "%.3f,%.3f,%.3f" bounds.Max.X bounds.Max.Y bounds.Max.Z

            let vS =
              geo.Vertices
              |> Array.map(fun v -> sprintf "%.3f,%.3f,%.3f" v.X v.Y v.Z)
              |> String.concat ","

            let iS = geo.Indices |> Array.map string |> String.concat ","

            sb.AppendLine(sprintf "%s|%s|%s|%s|%s" name minS maxS vS iS)
            |> ignore

          let path = Path.Combine(ctx.Content.RootDirectory, "collision.txt")
          File.WriteAllText(path, sb.ToString())

          printfn
            "Baking complete. Saved %d unique shapes to %s"
            shapes.Count
            path

        member _.Load(assetName: string) =
          if not(modelCache.ContainsKey assetName) then
            try
              let model = Assets.model assetName ctx
              modelCache.[assetName] <- model
              let key = getShapeKey assetName

              if not(geometryCache.ContainsKey key) then
                let bounds, geometry = extractModelData model
                boundsCache.[key] <- bounds
                geometryCache.[key] <- geometry

              match Mesh.fromModel model |> Seq.tryHead with
              | Some mesh -> meshCache.[assetName] <- mesh
              | None -> ()
            with ex ->
              printfn "Failed to load asset '%s': %O" assetName ex

        member _.Get(assetName: string) =
          match modelCache.TryGetValue assetName with
          | true, v -> Some v
          | _ -> None

        member _.GetMesh(assetName: string) =
          match meshCache.TryGetValue assetName with
          | true, v -> Some v
          | _ -> None

        member _.GetBounds(assetName: string) =
          match boundsCache.TryGetValue(getShapeKey assetName) with
          | true, v -> Some v
          | _ -> None

        member _.GetGeometry(assetName: string) =
          match geometryCache.TryGetValue(getShapeKey assetName) with
          | true, v -> Some v
          | _ -> None

        member _.LoadTexture(name) =
          if not(textureCache.ContainsKey name) then
            try
              textureCache.[name] <- Assets.texture name ctx
            with _ ->
              ()

        member _.GetTexture(name) =
          match textureCache.TryGetValue name with
          | true, v -> Some v
          | _ -> None
    }
