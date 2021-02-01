using UnityEngine;

public static class FalloffGenerator {

    public static float[,] GenerateFalloffMap(int width, int height) {
        var map = new float[width, height];

        for (var i = 0; i < width; i++) {
            for (var j = 0; j < height; j++) {
                var x = i / (float) width * 2 - 1;
                var y = j / (float) height * 2 - 1;

                var value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i, j] = Evaluate(value);
            }
        }

        return map;
    }

    private static float Evaluate(float value) {
        const float a = 2;
        const float b = 10;

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}
