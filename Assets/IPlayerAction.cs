using UnityEngine;
using System.Collections;

public interface IPlayerAction<T> {
    void ResolveAction(T t);  //destructive updates T.
}
