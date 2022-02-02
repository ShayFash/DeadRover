# RedRover
CMPT 406 project

[Google Drive](https://drive.google.com/drive/folders/1BNMBTIVkWm0pS4hRANQfSKkFriltXnwe?usp=sharing)

## Game Details

- Name: Dead Rover
- Inspiration: Red Rover
- Type of game: Turn based strategy combat

## Technical Conventions

- Branch names: use Jira number and a small description
  - `C4P-1/create-tilemap`
  - `update-readme` (if there is no associated Jira number)
- Commit messages use [conventional commits](https://www.conventionalcommits.org/en/v1.0.0/#summary)
  - Format: `type(scope if there is one): what you did`
    - `feat(main menu): create main menu`
    - `chore(pause menu): fix typo`
    - `refactor(AI): better readability`
- Coding Conventions
  - [Microsoft's C# coding conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
  - Basically what Visual Studio defaults to if you're using that (since it comes with Unity)
    - Note things like curly braces starting on newlines, otherwise pretty similar to other standards you may be use to
- Pull Requests
  - Generally in the form of the `Issue` being solved, and what was done to solve it
  - Typically use headers `### Issue` and `### Solution` in PRs with the appropriate section underneath the header
    - As redundant as `Issue: The pause menu had a typo, Solution: fix the typo`
    - Or something like `Issue: When selecting a unit there's a race condition causing the wrong unit to be selected, Solution: Remove the concurrency for selecting a unit because it's not needed`

## Guides

### Isometric Tilemap

- `Window > 2D > TilePalette` to create/access tile palettes
  - Save new palettes under `assets/palettes`
- Drag and drop tile palette textures into palette
  - If prompted (new palette with no tile textures yet) create a `bitmap` folder where file explorer opens up (this should be the same directory the tile textures are stored in
- In scene create `2D object > tilemap > isometric Z as Y` tilemap (for isometric squares allowing for 3D height effect)
  - Adjust tile anchors in Tilemap as necessary to center tiles in grid
  - Set tile to render individual tiles instead of chunks
  - Adjust grid settings as necessary to close/open gaps between tiles
- Ensure `Edit > Project Settings > Graphics` layers are set up appropriately (0, 1, 0 or some negative number depending on tile size)
