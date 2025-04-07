using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*******************************************************
* Code by: Alex Puigdengolas
*
* Class Purpose:
* GridManager is used to dynamically generate unique map layouts
* composed of variable-sized tiles while ensuring there are no empty gaps.
*******************************************************/
public class GridManager : MonoBehaviour
{
    [SerializeField] private int width, height;
    [SerializeField] private float spacing = 0.1f;
    [SerializeField] private Vector2Int minSize = new Vector2Int(1, 1);  
    [SerializeField] private Vector2Int maxSize = new Vector2Int(3, 3);  
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Transform camera;
    [SerializeField] private Transform tileContainer;

    private bool[,] occupied;
    private List<Tile> allTiles = new List<Tile>();

    void Awake()
    {
        // Warn if tile container isn't assigned to avoid hierarchy clutter
        if (tileContainer == null)
            Debug.LogWarning("Tile container is not assigned! Please set it in the inspector.");
    }

    void Start()
    {
        GenerateGrid();
        AdjustTilesToFillSpace();
        FillEmptySpaces();
        FillRemainingSingles();
    }

    /***
    * Generates the initial grid using randomly-sized tiles, ensuring that no tile overlaps.
    * Tiles are instantiated and positioned based on their size and spacing.
    ***/
    void GenerateGrid()
    {
        occupied = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int sizeX = Random.Range(minSize.x, maxSize.x + 1);
                int sizeY = Random.Range(minSize.y, maxSize.y + 1);
                Vector2Int tileSize = new Vector2Int(sizeX, sizeY);

                if (!CanPlaceTile(new Vector2Int(x, y), tileSize))
                    continue;

                MarkAreaAsOccupied(new Vector2Int(x, y), tileSize);

                float posX = x * (1 + spacing) + (tileSize.x - 1) * 0.5f;
                float posY = y * (1 + spacing) + (tileSize.y - 1) * 0.5f;

                var spawnedTile = Instantiate(tilePrefab, new Vector3(posX, posY, 0), Quaternion.identity, tileContainer);
                spawnedTile.origin = new Vector2Int(x, y);
                spawnedTile.name = $"Tile {x} {y} ({sizeX}x{sizeY})";
                spawnedTile.transform.localScale = new Vector3(sizeX, sizeY, 1);
                allTiles.Add(spawnedTile);

                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.Init(isOffset);
            }
        }

        // Center the camera on the grid
        camera.transform.position = new Vector3((width / 2f) - 0.5f, (height / 2f) - 0.5f, -10);
    }

    /***
    * Checks if a tile of the given size can be placed at the origin without overlapping other tiles.
    ***/
    bool CanPlaceTile(Vector2Int origin, Vector2Int size)
    {
        if (origin.x + size.x > width || origin.y + size.y > height)
            return false;

        for (int x = origin.x; x < origin.x + size.x; x++)
        {
            for (int y = origin.y; y < origin.y + size.y; y++)
            {
                if (occupied[x, y])
                    return false;
            }
        }
        return true;
    }

    /***
    * Marks an area on the grid as occupied.
    ***/
    void MarkAreaAsOccupied(Vector2Int origin, Vector2Int size)
    {
        for (int x = origin.x; x < origin.x + size.x; x++)
        {
            for (int y = origin.y; y < origin.y + size.y; y++)
            {
                occupied[x, y] = true;
            }
        }
    }

    /***
    * Attempts to expand existing tiles to remove gaps in the grid by growing them to the right and upward.
    ***/
    void AdjustTilesToFillSpace()
    {
        foreach (var tile in allTiles)
        {
            Vector2Int origin = tile.origin;
            Vector2Int currentSize = new Vector2Int(
                Mathf.RoundToInt(tile.transform.localScale.x),
                Mathf.RoundToInt(tile.transform.localScale.y)
            );

            UnmarkAreaAsOccupied(origin, currentSize);

            Vector2Int newSize = currentSize;

            while (CanExpand(tile, origin, newSize, Vector2Int.right))
                newSize.x++;

            while (CanExpand(tile, origin, newSize, Vector2Int.up))
                newSize.y++;

            if (newSize != currentSize)
            {
                tile.transform.localScale = new Vector3(newSize.x, newSize.y, 1);
                float newPosX = origin.x * (1 + spacing) + (newSize.x - 1) * 0.5f;
                float newPosY = origin.y * (1 + spacing) + (newSize.y - 1) * 0.5f;
                tile.transform.position = new Vector3(newPosX, newPosY, 0);
            }

            MarkAreaAsOccupied(origin, newSize);
        }
    }

    /***
    * Checks if the tile can expand in a given direction without colliding with occupied cells.
    ***/
    bool CanExpand(Tile tile, Vector2Int origin, Vector2Int size, Vector2Int direction)
    {
        if (direction == Vector2Int.right)
        {
            if (origin.x + size.x >= width) return false;
            for (int y = origin.y; y < origin.y + size.y; y++)
            {
                if (occupied[origin.x + size.x, y]) return false;
            }
        }
        else if (direction == Vector2Int.up)
        {
            if (origin.y + size.y >= height) return false;
            for (int x = origin.x; x < origin.x + size.x; x++)
            {
                if (occupied[x, origin.y + size.y]) return false;
            }
        }
        return true;
    }

    /***
    * Converts a world position to grid coordinates.
    ***/
    Vector2Int WorldToGrid(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / (1 + spacing));
        int y = Mathf.RoundToInt(position.y / (1 + spacing));
        return new Vector2Int(x, y);
    }

    /***
    * Fills large empty rectangular areas by spawning new tiles that cover as much space as possible.
    ***/
    void FillEmptySpaces()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (occupied[x, y]) continue;

                Vector2Int maxPossibleSize = GetMaxEmptyArea(new Vector2Int(x, y));
                if (maxPossibleSize.x == 0 || maxPossibleSize.y == 0) continue;

                MarkAreaAsOccupied(new Vector2Int(x, y), maxPossibleSize);

                float posX = x * (1 + spacing) + (maxPossibleSize.x - 1) * 0.5f;
                float posY = y * (1 + spacing) + (maxPossibleSize.y - 1) * 0.5f;

                var tile = Instantiate(tilePrefab, new Vector3(posX, posY, 0), Quaternion.identity, tileContainer);
                tile.name = $"[FILL] Tile {x} {y} ({maxPossibleSize.x}x{maxPossibleSize.y})";
                tile.transform.localScale = new Vector3(maxPossibleSize.x, maxPossibleSize.y, 1);
                tile.Init((x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0));
                allTiles.Add(tile);
            }
        }
    }

    /***
    * Calculates the maximum available empty area starting from a given grid cell.
    ***/
    Vector2Int GetMaxEmptyArea(Vector2Int start)
    {
        int maxWidth = 0;
        int maxHeight = 0;

        for (int x = start.x; x < width && !occupied[x, start.y]; x++)
            maxWidth++;

        if (maxWidth == 0) return Vector2Int.zero;

        for (int y = start.y; y < height; y++)
        {
            for (int x = start.x; x < start.x + maxWidth; x++)
            {
                if (occupied[x, y])
                    return new Vector2Int(maxWidth, maxHeight);
            }
            maxHeight++;
        }

        return new Vector2Int(maxWidth, maxHeight);
    }

    /***
    * Fills any remaining single empty cells with 1x1 tiles to complete the grid.
    ***/
    void FillRemainingSingles()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!occupied[x, y])
                {
                    MarkAreaAsOccupied(new Vector2Int(x, y), Vector2Int.one);

                    float posX = x * (1 + spacing);
                    float posY = y * (1 + spacing);

                    var tile = Instantiate(tilePrefab, new Vector3(posX, posY, 0), Quaternion.identity, tileContainer);
                    tile.name = $"[SINGLE] Tile {x} {y} (1x1)";
                    tile.transform.localScale = Vector3.one;
                    tile.Init(false);
                    allTiles.Add(tile);
                }
            }
        }
    }

    /***
    * Unmarks an area as occupied. Useful for resizing or repositioning tiles.
    ***/
    void UnmarkAreaAsOccupied(Vector2Int origin, Vector2Int size)
    {
        for (int x = origin.x; x < origin.x + size.x; x++)
        {
            for (int y = origin.y; y < origin.y + size.y; y++)
            {
                occupied[x, y] = false;
            }
        }
    }
}
