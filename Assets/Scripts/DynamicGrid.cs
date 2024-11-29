using UnityEngine;
using UnityEngine.UI;

public class DynamicGrid : MonoBehaviour
{
    public GridLayoutGroup gridLayoutGroup;
    public RectTransform container;
    public int rows = 2;
    public int columns = 2;

    void Start()
    {
        AdjustGridLayout();
    }

    public void SetGridLayout(int newRows, int newColumns)
    {
        rows = newRows;
        columns = newColumns;
        AdjustGridLayout();
    }

    void AdjustGridLayout()
    {
        // Calculate cell size based on container size
        float width = container.rect.width / columns;
        float height = container.rect.height / rows;

        //gridLayoutGroup.cellSize = new Vector2(width, height);
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = columns;
    }
}