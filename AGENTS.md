# AGENTS.md - Development Guidelines for FsBump

## Build/Test Commands

### Build

Since we have an android project we need to call `dotnet workload restore` note however, that it might need administration privileges, so notify user in case the program fails due missing workloads.

```bash
# Build all projects
dotnet build

# Build specific project
dotnet build src/FsBump.Core/FsBump.Core.fsproj
dotnet build src/FsBump.Desktop/FsBump.Desktop.fsproj
dotnet build src/FsBump.Android/FsBump.Android.fsproj

# Build for release
dotnet build -c Release
```

### Run
```bash
# Run Desktop app
dotnet run --project src/FsBump.Desktop/FsBump.Desktop.fsproj
```

### Clean
```bash
# Clean build artifacts
dotnet clean
```

### Restore
```bash
# Restore packages
dotnet restore
```

### Note on Testing
This project currently does not have a test suite. When adding tests:
- Create a test project using `dotnet new xunit -lang F#` or similar
- Follow the existing file structure (place tests in dedicated test directory)
- Use Xunit for .NET testing with F#

---

## Code Style Guidelines

### File Structure
- Files start with `namespace` declaration
- Imports (`open`) follow immediately, topologically sorted namespaces. e.g. System, System.IO, System.Threading then Microsoft.*.
- open statements shadow declarations, so they might need a different sorting order based on the User's definitions in the FsBump.Core.fsproj file
- Semantic modules and submodules with well defined and responsible functions.
- Avoid mega functions with large bodies, refactor in composable functions that favor partial application.

### Formatting (from .editorconfig)
- Indentation: 2 spaces
- Max line length: 80 characters
- Multiline brackets: Stroustrup style
- No space before parameters
- No space before lowercase invocation
- Newline before multiline computation expression: false

### Types and Naming
- **Types**: PascalCase (e.g., `PlayerModel`, `TileType`, `PathGraph`)
- **Modules**: PascalCase (e.g., `Physics`, `Player`, `Movement`)
- **Functions**: camelCase (e.g., `create`, `updateTick`, `getAssetData`)
- **Constants**: PascalCase or upper case as needed (e.g., `Gravity`, `SaturnRings`)

### Type Definitions
- Prefer struct objects based on usage pattern and presence in hot paths.
- Use `[<Struct>]` for performance-critical types (frequently used in arrays/collections)
- Use records for data containers: `{ Position: Vector3; Velocity: Vector3 }`
- Use Discriminated Unions for closed sets: `| Blue | Green | Red | Yellow`
- Use `[<Literal>]` for string constants
- Apply Units of Measure where appropriate: `type [<Measure>] PathId`, `Guid<PathId>`
- Avoid more than 3 parameters per function, use contextual objects instead.
- Prefer object expressions `{ new Interface with ... impl ...}` to concrete classes
- Delare types before they are used

### Error Handling
- **Prefer `ValueOption`** over `Option` for performance (no allocation in None case)
- Pattern match on ValueOption: `| ValueSome x -> ... | ValueNone -> ...`
- Return `ValueSome(value)` or `ValueNone` for optional results
- Use `ValueOption.iter` and `ValueOption.bind` for composition

### Pattern Matching
- Use pattern matching extensively instead of if/else
- Match on DUs: `match tile.Type with | Floor -> ... | Wall -> ...`
- Match on record properties: `{ model with Position = newPos }`
- Use function-style matching for transformations

### Module Organization
- **Config modules**: Nested under feature (e.g., `Player.Config.Physics`, `Player.Config.Visuals`)
- **Input modules**: Handle input mapping (e.g., `Player.Input.config`)
- **Logic modules**: Core business logic (e.g., `Player.Logic.updateTick`)
- **View modules**: Rendering logic (e.g., `Player.View.getLight`, `Player.View.drawPlayer`)
- **Private modules**: Implementation details (e.g., `module private Internal`)

### Elmish Architecture
- Follow init/update/view pattern for game state management
- `init`: Returns `(initialModel, Cmd.none)`
- `update`: Returns `(updatedModel, Cmd.batch(...))`
- `view`: Renders to buffer
- Use `Cmd.ofMsg`, `Cmd.map`, `Cmd.batch`, `Cmd.batch2` for commands

### Performance Guidelines
- Use `[<Struct>]` for small, frequently created types
- Use `ResizeArray<T>` instead of `List<T>` when mutable collections needed
- Use `struct` returns with `struct (value1, value2)` to avoid allocation
- Use `ValueOption` for optional values
- Cache frequently accessed values (e.g., `let len = array.Length`)
- Use inline for small, frequently called functions (e.g., `let inline createName ...`)

### Constants and Config
- Group related constants in `module Config` nested under feature
- Separate concerns: `Physics` constants vs `Visuals` constants
- Use float32 literals with `f` suffix: `1.0f`, `0.5f`
- Use `MathHelper.ToRadians` for angle conversions

### Comments and Documentation
- Minimal inline comments - code should be self-documenting
- Use XML comments for public APIs when needed
- Use section separators sparingly: `// ───────────────────────────────────`
- Comment non-obvious algorithms or complex math

### Interop with External Libraries
- **MonoGame**: Vector3, Vector2, Matrix, Quaternion, Color types from `Microsoft.Xna.Framework`
- **Mibo**: Rendering, Input, Elmish utilities
- **FSharp.UMX**: Type-safe unit wrappers (`Guid<'Measure>`)

### Asset Management
- Use `AssetDefinition` with `AssetLocation` discriminated union
- Pattern: `Colored(colorVariant, assetNamingPattern, assetName)` or `Neutral assetName`
- Load assets via `IModelStore.Load` / `IModelStore.Get`

### Physics/Math
- Vector operations use MonoGame Vector types
- Use `Vector3.DistanceSquared` for comparisons (avoids sqrt)
- Use `Math.Clamp`, `Math.Max`, `Math.Min` for bounded values
- Normalize vectors after creation if direction needed

### File Compilation Order
- Files are compiled in order specified in `.fsproj`
- Define dependencies first (e.g., `Domain.fs` first)
- Put types that other files depend on earlier in compilation order
- Put types before their usage.
- Do not add circular references (type <type> `and` <type>) unless explicit recursion is required.

### Commit Message Style
- Use conventional commits: `feat(scope): description`, `fix(scope): description`, `chore: ...`
- See recent commits: `feat(assets): refactor asset management`, `docs(design): add Infinite Park World design`
