using System;

public class Either<TL, TR> {
    readonly TL left;
    readonly TR right;
    readonly bool isLeft;

    public Either(TL left) {
        this.left = left;
        isLeft = true;
    }

    public Either(TR right) {
        this.right = right;
        isLeft = false;
    }

    public T Match<T>(Func<TL, T> leftFunc, Func<TR, T> rightFunc)
        => isLeft ? leftFunc(left) : rightFunc(right);

    public static implicit operator Either<TL, TR>(TL left) => new Either<TL, TR>(left);

    public static implicit operator Either<TL, TR>(TR right) => new Either<TL, TR>(right);
}