using OneCardExpenseValidator.API.Models;

namespace OneCardExpenseValidator.API.Services;

public interface ITicketParserService
{
    /// <summary>
    /// Parsea el texto extraído del OCR y extrae información del ticket
    /// </summary>
    /// <param name="ocrText">Texto extraído del OCR</param>
    /// <returns>Información parseada del ticket</returns>
    ParsedTicketData ParseTicketText(string ocrText);
}
