using System.Globalization;
using System.Text.RegularExpressions;
using OneCardExpenseValidator.API.Models;

namespace OneCardExpenseValidator.API.Services;

public class TicketParserService : ITicketParserService
{
    private readonly ILogger<TicketParserService> _logger;

    public TicketParserService(ILogger<TicketParserService> logger)
    {
        _logger = logger;
    }

    public ParsedTicketData ParseTicketText(string ocrText)
    {
        _logger.LogInformation("Parsing ticket text");

        var parsedData = new ParsedTicketData
        {
            RawText = ocrText
        };

        try
        {
            // Extraer vendedor (buscar palabras clave comunes)
            parsedData.Vendor = ExtractVendor(ocrText);

            // Extraer fecha
            parsedData.TicketDate = ExtractDate(ocrText);

            // Extraer total
            parsedData.TotalAmount = ExtractTotal(ocrText);

            // Extraer items/productos
            parsedData.Items = ExtractItems(ocrText);

            _logger.LogInformation($"Parsed ticket: Vendor={parsedData.Vendor}, Date={parsedData.TicketDate}, Total={parsedData.TotalAmount}, Items={parsedData.Items.Count}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing ticket text");
        }

        return parsedData;
    }

    private string? ExtractVendor(string text)
    {
        // Buscar nombres de tiendas comunes
        var vendors = new[]
        {
            "SORIANA", "WALMART", "HEB", "CHEDRAUI", "BODEGA AURRERA",
            "OXXO", "7-ELEVEN", "COSTCO", "SAM'S CLUB", "SUPERAMA",
            "LA COMER", "CITY CLUB", "COMERCIAL MEXICANA"
        };

        foreach (var vendor in vendors)
        {
            if (text.Contains(vendor, StringComparison.OrdinalIgnoreCase))
            {
                return vendor;
            }
        }

        // Si no se encuentra un vendedor conocido, buscar en las primeras líneas
        var lines = text.Split('\n');
        if (lines.Length > 0)
        {
            var firstLine = lines[0].Trim();
            if (!string.IsNullOrWhiteSpace(firstLine) && firstLine.Length < 50)
            {
                return firstLine;
            }
        }

        return null;
    }

    private DateTime? ExtractDate(string text)
    {
        // Patrones comunes de fecha en tickets mexicanos
        var datePatterns = new[]
        {
            @"(\d{1,2})[/-](\d{1,2})[/-](\d{2,4})",  // DD/MM/YYYY o DD-MM-YYYY
            @"(\d{2,4})[/-](\d{1,2})[/-](\d{1,2})",  // YYYY/MM/DD
            @"(\d{1,2})\s+(?:de\s+)?([A-Za-z]+)\s+(?:de\s+)?(\d{2,4})"  // DD de MMMM de YYYY
        };

        foreach (var pattern in datePatterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                try
                {
                    // Intentar parsear la fecha
                    if (pattern.Contains("[A-Za-z]"))
                    {
                        // Fecha con nombre de mes
                        var day = int.Parse(match.Groups[1].Value);
                        var monthName = match.Groups[2].Value;
                        var year = int.Parse(match.Groups[3].Value);

                        if (year < 100) year += 2000;

                        var monthMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                        {
                            {"enero", 1}, {"ene", 1},
                            {"febrero", 2}, {"feb", 2},
                            {"marzo", 3}, {"mar", 3},
                            {"abril", 4}, {"abr", 4},
                            {"mayo", 5}, {"may", 5},
                            {"junio", 6}, {"jun", 6},
                            {"julio", 7}, {"jul", 7},
                            {"agosto", 8}, {"ago", 8},
                            {"septiembre", 9}, {"sep", 9}, {"sept", 9},
                            {"octubre", 10}, {"oct", 10},
                            {"noviembre", 11}, {"nov", 11},
                            {"diciembre", 12}, {"dic", 12}
                        };

                        if (monthMap.TryGetValue(monthName, out int month))
                        {
                            return new DateTime(year, month, day);
                        }
                    }
                    else
                    {
                        // Fecha numérica
                        var parts = new[] { match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value };

                        // Determinar si es DD/MM/YYYY o YYYY/MM/DD
                        if (parts[0].Length == 4)
                        {
                            // YYYY/MM/DD
                            var year = int.Parse(parts[0]);
                            var month = int.Parse(parts[1]);
                            var day = int.Parse(parts[2]);
                            return new DateTime(year, month, day);
                        }
                        else
                        {
                            // DD/MM/YYYY
                            var day = int.Parse(parts[0]);
                            var month = int.Parse(parts[1]);
                            var year = int.Parse(parts[2]);

                            if (year < 100) year += 2000;

                            return new DateTime(year, month, day);
                        }
                    }
                }
                catch
                {
                    // Si falla el parsing, continuar con el siguiente patrón
                    continue;
                }
            }
        }

        return null;
    }

    private decimal? ExtractTotal(string text)
    {
        // Buscar patrones de total (tolerantes con OCR)
        var totalPatterns = new[]
        {
            @"TOTAL[:\s]*\$?\s*(\d{1,3}(?:[,\s]\d{3})*(?:[.,]\d{2})?)",
            @"T[O0]TAL[:\s]*\$?\s*(\d{1,3}(?:[,\s]\d{3})*(?:[.,]\d{2})?)",
            @"IMPORTE[:\s]*\$?\s*(\d{1,3}(?:[,\s]\d{3})*(?:[.,]\d{2})?)",
            @"A\s+PAGAR[:\s]*\$?\s*(\d{1,3}(?:[,\s]\d{3})*(?:[.,]\d{2})?)",
            @"GRAN\s+TOTAL[:\s]*\$?\s*(\d{1,3}(?:[,\s]\d{3})*(?:[.,]\d{2})?)"
        };

        foreach (var pattern in totalPatterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                try
                {
                    var totalStr = match.Groups[1].Value
                        .Replace(",", "")
                        .Replace(" ", "");

                    if (decimal.TryParse(totalStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal total))
                    {
                        return total;
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        return null;
    }

    private List<ParsedTicketItem> ExtractItems(string text)
    {
        var items = new List<ParsedTicketItem>();
        var lines = text.Split('\n');

        // Patrón para líneas de productos:
        // CANTIDAD DESCRIPCION PRECIO UNITARIO TOTAL
        // Ejemplo: 1 POLLO ROSTIZADO 250.00 250.00
        var itemPattern = @"^(\d+)\s+([A-Za-z0-9\s\-\/]+?)\s+(\d{1,3}(?:,\d{3})*(?:\.\d{2})?)\s+(\d{1,3}(?:,\d{3})*(?:\.\d{2})?)";

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Ignorar líneas muy cortas o que contienen palabras clave de encabezado/pie
            if (trimmedLine.Length < 5 ||
                ContainsIgnoreKeywords(trimmedLine))
            {
                continue;
            }

            var match = Regex.Match(trimmedLine, itemPattern);
            if (match.Success)
            {
                try
                {
                    var quantity = int.Parse(match.Groups[1].Value);
                    var description = match.Groups[2].Value.Trim();
                    var unitPriceStr = match.Groups[3].Value.Replace(",", "");
                    var totalPriceStr = match.Groups[4].Value.Replace(",", "");

                    if (decimal.TryParse(unitPriceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal unitPrice) &&
                        decimal.TryParse(totalPriceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal totalPrice))
                    {
                        items.Add(new ParsedTicketItem
                        {
                            Description = description,
                            Quantity = quantity,
                            UnitPrice = unitPrice,
                            TotalPrice = totalPrice
                        });
                    }
                }
                catch
                {
                    // Si falla el parsing de esta línea, continuar con la siguiente
                    continue;
                }
            }
            else
            {
                // Patrón alternativo: DESCRIPCION PRECIO (sin cantidad explícita)
                var simplePattern = @"^([A-Za-z0-9\s\-\/]{3,}?)\s+(\d{1,3}(?:,\d{3})*(?:\.\d{2}))$";
                var simpleMatch = Regex.Match(trimmedLine, simplePattern);

                if (simpleMatch.Success)
                {
                    try
                    {
                        var description = simpleMatch.Groups[1].Value.Trim();
                        var priceStr = simpleMatch.Groups[2].Value.Replace(",", "");

                        if (decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price) &&
                            price > 0 && price < 100000) // Validación de rango razonable
                        {
                            items.Add(new ParsedTicketItem
                            {
                                Description = description,
                                Quantity = 1,
                                UnitPrice = price,
                                TotalPrice = price
                            });
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }

        return items;
    }

    private bool ContainsIgnoreKeywords(string line)
    {
        var ignoreKeywords = new[]
        {
            "TOTAL", "SUBTOTAL", "IVA", "IMPORTE", "CAMBIO", "EFECTIVO", "TARJETA",
            "TICKET", "FOLIO", "FECHA", "HORA", "CAJERO", "CAJA", "GRACIAS",
            "RFC", "DOMICILIO", "TELEFONO", "ARTICULOS", "CANTIDAD", "DESCRIPCION",
            "PRECIO", "DESCUENTO", "PUNTOS", "AHORRO", "FIRMA"
        };

        return ignoreKeywords.Any(keyword => line.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}
