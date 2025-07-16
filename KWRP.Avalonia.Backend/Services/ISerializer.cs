namespace KWRP.Avalonia.Backend.Services
{
    public interface ISerializer<T>
    {
        Task SaveAsync(T item, string filePath);
        Task<T?> LoadAsync(string filePath);

        T? Load(string filePath);
    }
}
