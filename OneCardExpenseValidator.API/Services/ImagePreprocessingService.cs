using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace OneCardExpenseValidator.API.Services;

public class ImagePreprocessingService : IImagePreprocessingService
{
    private readonly ILogger<ImagePreprocessingService> _logger;

    public ImagePreprocessingService(ILogger<ImagePreprocessingService> logger)
    {
        _logger = logger;
    }

    public async Task<MemoryStream> PreprocessImageAsync(Stream inputStream)
    {
        try
        {
            _logger.LogInformation("Starting image preprocessing");

            using var image = await Image.LoadAsync<Rgb24>(inputStream);

            // Aplicar mejoras suaves de imagen
            image.Mutate(x => x
                // 1. Convertir a escala de grises para mejor reconocimiento
                .Grayscale()
                // 2. Aumentar el tamaño si es muy pequeña
                .Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(1500, 1500) // Resolución moderada
                })
                // 3. Aumentar contraste moderadamente
                .Contrast(1.2f)
                // 4. Aplicar nitidez suave
                .GaussianSharpen(0.8f)
            );

            // Guardar en un MemoryStream
            var outputStream = new MemoryStream();
            await image.SaveAsPngAsync(outputStream);
            outputStream.Position = 0;

            _logger.LogInformation($"Image preprocessing completed. Output size: {outputStream.Length} bytes");

            return outputStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preprocessing image");
            throw;
        }
    }

    public async Task<MemoryStream> PreprocessImageFromPathAsync(string imagePath)
    {
        try
        {
            using var fileStream = File.OpenRead(imagePath);
            return await PreprocessImageAsync(fileStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error preprocessing image from path: {imagePath}");
            throw;
        }
    }
}
