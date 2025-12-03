namespace OneCardExpenseValidator.API.Services;

public interface IImagePreprocessingService
{
    /// <summary>
    /// Preprocesa una imagen para mejorar la calidad del OCR
    /// </summary>
    /// <param name="inputStream">Stream de la imagen original</param>
    /// <returns>Stream de la imagen procesada</returns>
    Task<MemoryStream> PreprocessImageAsync(Stream inputStream);

    /// <summary>
    /// Preprocesa una imagen desde una ruta
    /// </summary>
    /// <param name="imagePath">Ruta de la imagen</param>
    /// <returns>Stream de la imagen procesada</returns>
    Task<MemoryStream> PreprocessImageFromPathAsync(string imagePath);
}
