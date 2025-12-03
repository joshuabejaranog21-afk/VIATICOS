using Tesseract;

namespace OneCardExpenseValidator.API.Services;

public class TesseractOcrService : IOcrService
{
    private readonly string _tessDataPath;
    private readonly ILogger<TesseractOcrService> _logger;

    public TesseractOcrService(ILogger<TesseractOcrService> logger, IConfiguration configuration)
    {
        _logger = logger;
        // Ruta donde están los archivos de datos de Tesseract
        _tessDataPath = configuration["Tesseract:DataPath"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");

        if (!Directory.Exists(_tessDataPath))
        {
            _logger.LogWarning($"Tesseract data path does not exist: {_tessDataPath}");
        }
    }

    public async Task<string> ExtractTextFromImageAsync(string imagePath)
    {
        try
        {
            _logger.LogInformation($"Extracting text from image: {imagePath}");

            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException($"Image file not found: {imagePath}");
            }

            using var engine = new TesseractEngine(_tessDataPath, "spa", EngineMode.Default);
            using var img = Pix.LoadFromFile(imagePath);
            using var page = engine.Process(img);

            var text = page.GetText();
            var confidence = page.GetMeanConfidence();

            _logger.LogInformation($"OCR completed with confidence: {confidence:P}");

            return await Task.FromResult(text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from image");
            throw;
        }
    }

    public async Task<string> ExtractTextFromStreamAsync(Stream imageStream)
    {
        try
        {
            _logger.LogInformation("Extracting text from stream");

            using var engine = new TesseractEngine(_tessDataPath, "spa", EngineMode.Default);

            // Convertir el stream a byte array para Pix
            using var memoryStream = new MemoryStream();
            await imageStream.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();

            using var img = Pix.LoadFromMemory(imageBytes);
            using var page = engine.Process(img);

            var text = page.GetText();
            var confidence = page.GetMeanConfidence();

            _logger.LogInformation($"OCR completed with confidence: {confidence:P}");

            return text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from stream");
            throw;
        }
    }
}
