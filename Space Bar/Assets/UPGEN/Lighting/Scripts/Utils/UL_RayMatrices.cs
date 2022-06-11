using UnityEngine;

public static class UL_RayMatrices
{
    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private static readonly Vector2[] GRID_2x2 = GenerateGrid(2, 2);
    private static readonly Vector2[] GRID_3x3 = GenerateGrid(3, 3);
    private static readonly Vector2[] GRID_4x4 = GenerateGrid(4, 4);
    private static readonly Vector2[] GRID_5x5 = GenerateGrid(5, 5);
    private static readonly Vector2[] GRID_6x6 = GenerateGrid(6, 6);
    private static readonly Vector2[] GRID_7x7 = GenerateGrid(7, 7);
    private static readonly Vector2[] GRID_8x8 = GenerateGrid(8, 8);
    private static readonly Vector2[] GRID_9x9 = GenerateGrid(9, 9);
    private static readonly Vector2[] GRID_10x10 = GenerateGrid(10, 10);
    private static readonly Vector2[] GRID_11x11 = GenerateGrid(11, 11);
    private static readonly Vector2[] GRID_12x12 = GenerateGrid(12, 12);
    private static readonly Vector2[] GRID_13x13 = GenerateGrid(13, 13);
    private static readonly Vector2[] GRID_14x14 = GenerateGrid(14, 14);
    private static readonly Vector2[] GRID_15x15 = GenerateGrid(15, 15);
    private static readonly Vector2[] GRID_16x16 = GenerateGrid(16, 16);
    private static readonly Vector2[] GRID_17x17 = GenerateGrid(17, 17);
    private static readonly Vector2[] GRID_18x18 = GenerateGrid(18, 18);
    private static readonly Vector2[] GRID_19x19 = GenerateGrid(19, 19);
    private static readonly Vector2[] GRID_20x20 = GenerateGrid(20, 20);
    private static readonly Vector2[] GRID_21x21 = GenerateGrid(21, 21);
    private static readonly Vector2[] GRID_22x22 = GenerateGrid(22, 22);
    private static readonly Vector2[] GRID_23x23 = GenerateGrid(23, 23);
    private static readonly Vector2[] GRID_24x24 = GenerateGrid(24, 24);
    private static readonly Vector2[] GRID_25x25 = GenerateGrid(25, 25);

    public static readonly Vector2[][] GRID = { GRID_2x2, GRID_3x3, GRID_4x4, GRID_5x5, GRID_6x6, GRID_7x7, GRID_8x8, GRID_9x9, GRID_10x10, GRID_11x11, GRID_12x12, GRID_13x13, GRID_14x14, GRID_15x15, GRID_16x16, GRID_17x17, GRID_18x18, GRID_19x19, GRID_20x20, GRID_21x21, GRID_22x22, GRID_23x23, GRID_24x24, GRID_25x25 };

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private static Vector2[] GenerateGrid(int width, int height)
    {
        var items = new Vector2[width * height];
        var wStep = 1f / width;
        var hStep = 1f / height;
        var cnt = 0;
        for (var w = 0; w < width; w++)
        for (var h = 0; h < height; h++)
            items[cnt++] = new Vector2(2f * (w + 0.5f) * wStep - 1, 2f * (h + 0.5f) * hStep - 1);
        return items;
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private static readonly Vector3[] SPHERE_2x2 = GenerateSphere(2 * 2);
    private static readonly Vector3[] SPHERE_3x3 = GenerateSphere(3 * 3);
    private static readonly Vector3[] SPHERE_4x4 = GenerateSphere(4 * 4);
    private static readonly Vector3[] SPHERE_5x5 = GenerateSphere(5 * 5);
    private static readonly Vector3[] SPHERE_6x6 = GenerateSphere(6 * 6);
    private static readonly Vector3[] SPHERE_7x7 = GenerateSphere(7 * 7);
    private static readonly Vector3[] SPHERE_8x8 = GenerateSphere(8 * 8);
    private static readonly Vector3[] SPHERE_9x9 = GenerateSphere(9 * 9);
    private static readonly Vector3[] SPHERE_10x10 = GenerateSphere(10 * 10);
    private static readonly Vector3[] SPHERE_11x11 = GenerateSphere(11 * 11);
    private static readonly Vector3[] SPHERE_12x12 = GenerateSphere(12 * 12);
    private static readonly Vector3[] SPHERE_13x13 = GenerateSphere(13 * 13);
    private static readonly Vector3[] SPHERE_14x14 = GenerateSphere(14 * 14);
    private static readonly Vector3[] SPHERE_15x15 = GenerateSphere(15 * 15);
    private static readonly Vector3[] SPHERE_16x16 = GenerateSphere(16 * 16);
    private static readonly Vector3[] SPHERE_17x17 = GenerateSphere(17 * 17);
    private static readonly Vector3[] SPHERE_18x18 = GenerateSphere(18 * 18);
    private static readonly Vector3[] SPHERE_19x19 = GenerateSphere(19 * 19);
    private static readonly Vector3[] SPHERE_20x20 = GenerateSphere(20 * 20);
    private static readonly Vector3[] SPHERE_21x21 = GenerateSphere(21 * 21);
    private static readonly Vector3[] SPHERE_22x22 = GenerateSphere(22 * 22);
    private static readonly Vector3[] SPHERE_23x23 = GenerateSphere(23 * 23);
    private static readonly Vector3[] SPHERE_24x24 = GenerateSphere(24 * 24);
    private static readonly Vector3[] SPHERE_25x25 = GenerateSphere(25 * 25);

    public static readonly Vector3[][] SPHERE = { SPHERE_2x2, SPHERE_3x3, SPHERE_4x4, SPHERE_5x5, SPHERE_6x6, SPHERE_7x7, SPHERE_8x8, SPHERE_9x9, SPHERE_10x10, SPHERE_11x11, SPHERE_12x12, SPHERE_13x13, SPHERE_14x14, SPHERE_15x15, SPHERE_16x16, SPHERE_17x17, SPHERE_18x18, SPHERE_19x19, SPHERE_20x20, SPHERE_21x21, SPHERE_22x22, SPHERE_23x23, SPHERE_24x24, SPHERE_25x25 };

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private static Vector3[] GenerateSphere(int count)
    {
        var pts = new Vector3[count];
        var inc = Mathf.PI * (3 - Mathf.Sqrt(5));
        var off = 2f / count;
        for (var i = 0; i < count; i++)
        {
            var y = i * off - 1 + off / 2;
            var r = Mathf.Sqrt(1 - y * y);
            var phi = i * inc;
            var x = Mathf.Cos(phi) * r;
            var z = Mathf.Sin(phi) * r;
            pts[i] = new Vector3(x, y, z);
        }
        return pts;
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}