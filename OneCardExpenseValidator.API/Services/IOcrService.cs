namespace OneCardExpenseValidator.API.Services;

public interface IOcrService
{
    /// <summary>
    /// Extrae texto de una imagen usando OCR
    /// </summary>
    /// <param name="imagePath">Ruta de la imagen</param>
    /// <returns>Texto extraído de la imagen</returns>
    Task<string> ExtractTextFromImageAsync(string imagePath);

    /// <summary>
    /// Extrae texto de un stream de imagen
    /// </summary>
    /// <param name="imageStream">Stream de la imagen</param>
    /// <returns>Texto extraído de la imagen</returns>
    Task<string> ExtractTextFromStreamAsync(Stream imageStream);
}
