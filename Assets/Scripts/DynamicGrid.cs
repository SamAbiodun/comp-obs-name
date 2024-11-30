using UnityEngine;
using UnityEngine.UI;

public class DynamicGrid : MonoBehaviour
{
    [SerializeField] private SpawnCards spawnCards;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private RectTransform container;

    [Range(0.1f, 3f)] 
    public float aspectRatio = 1.0f; //adjust based on card height/width ratio

    private int rows;
    private int columns;

    void Start()
    {
        rows = spawnCards.rows;
        columns = spawnCards.columns;
        AdjustGridLayout();
    }

    void AdjustGridLayout()
    {
        //space for padding
        float totalHorizontalSpacing = gridLayoutGroup.spacing.x * (columns - 1);
        float totalVerticalSpacing = gridLayoutGroup.spacing.y * (rows - 1);

        //pad size
        float totalHorizontalPadding = gridLayoutGroup.padding.left + gridLayoutGroup.padding.right;
        float totalVerticalPadding = gridLayoutGroup.padding.top + gridLayoutGroup.padding.bottom;

        //cell spaces
        float availableWidth = container.rect.width - totalHorizontalSpacing - totalHorizontalPadding;
        float availableHeight = container.rect.height - totalVerticalSpacing - totalVerticalPadding;

        //cell size
        float cellWidth = availableWidth / columns;
        float cellHeight = availableHeight / rows;

        //aspect ratio??
        if (aspectRatio > 0)
        {
            cellHeight = Mathf.Min(cellHeight, cellWidth * aspectRatio);
        }

        //assign cell size and grid constraints
        gridLayoutGroup.cellSize = new Vector2(cellWidth, cellHeight);
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = columns;
    }

    //update at runtime
    public void UpdateLayout(int newRows, int newColumns)
    {
        rows = newRows;
        columns = newColumns;
        AdjustGridLayout();
    }
}
