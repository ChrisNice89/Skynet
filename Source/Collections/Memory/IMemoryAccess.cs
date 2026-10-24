namespace Skynet.Memory;

internal interface IMemoryAccess<T>
{
    ref T At(int index);
}