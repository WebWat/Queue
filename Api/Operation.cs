namespace Api
{
    public class Operation
    {
        public static double Determinant(double[,] _array)
        {
            if (_array.GetLength(0) != _array.GetLength(1))
                throw new ArgumentException("The matrix must be square");

            switch (_array.GetLength(0))
            {
                case 1:
                    return _array[0, 0];
                case 2:
                    return _array[0, 0] * _array[1, 1] - _array[0, 1] * _array[1, 0];
                case 3:
                    return _array[0, 0] * _array[1, 1] * _array[2, 2] +
                           _array[0, 1] * _array[1, 2] * _array[2, 0] +
                           _array[0, 2] * _array[1, 0] * _array[2, 1] -
                           _array[0, 2] * _array[1, 1] * _array[2, 0] -
                           _array[0, 0] * _array[1, 2] * _array[2, 1] -
                           _array[0, 1] * _array[1, 0] * _array[2, 2];
                default:
                    double result = 0;

                    for (int i = 0; i < _array.GetLength(1); i++)
                    {
                        double value = _array[0, i];
                        var matrix = new double[_array.GetLength(1) - 1, _array.GetLength(1) - 1];
                        int column = 0;

                        for (int a = 1; a < _array.GetLength(0); a++)
                        {
                            for (int b = 0; b < _array.GetLength(1); b++)
                            {
                                if (b != i)
                                {
                                    matrix[a - 1, column] = _array[a, b];
                                    column++;
                                }
                            }
                            column = 0;
                        }

                        result += (i % 2 == 0 ? 1 : (-1)) * value * Determinant(matrix);
                    }

                    return result;
            }
        }
    }
}
