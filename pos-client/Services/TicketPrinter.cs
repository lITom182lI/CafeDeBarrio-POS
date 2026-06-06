using System.Drawing.Printing;

namespace CafeBarrio.POS.Services;

public static class TicketPrinter
{
    private const int PageWidthPx = 260;

    public static void Print(
        List<(int ProductoId, string Nombre, decimal Precio, int Cantidad)> items,
        decimal subtotal,
        decimal igv,
        decimal total,
        string metodoPago)
    {
        var lines = BuildLines(items, subtotal, igv, total, metodoPago);
        var doc = new PrintDocument();
        doc.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);
        doc.PrintPage += (_, e) =>
        {
            if (e.Graphics is null) return;
            var g = e.Graphics;
            var mono   = new Font("Courier New", 8f);
            var bold   = new Font("Courier New", 8f, FontStyle.Bold);
            var header = new Font("Courier New", 10f, FontStyle.Bold);
            var brush  = Brushes.Black;

            float y = 0;
            foreach (var (text, style) in lines)
            {
                var f = style switch
                {
                    LineStyle.Header  => header,
                    LineStyle.Bold    => bold,
                    _                 => mono
                };
                g.DrawString(text, f, brush, 0, y);
                y += f.GetHeight(g) + 1;
            }

            mono.Dispose();
            bold.Dispose();
            header.Dispose();
        };

        try { doc.Print(); }
        catch { /* no romper el flujo si falla la impresora */ }
        finally { doc.Dispose(); }
    }

    private static List<(string Text, LineStyle Style)> BuildLines(
        List<(int ProductoId, string Nombre, decimal Precio, int Cantidad)> items,
        decimal subtotal, decimal igv, decimal total, string metodoPago)
    {
        const string sep  = "--------------------------------";
        const string sep2 = "================================";
        var now = DateTime.Now;
        var ls  = new List<(string, LineStyle)>();

        ls.Add((sep2,                                                    LineStyle.Normal));
        ls.Add((Center("CAFE DE BARRIO"),                                LineStyle.Header));
        ls.Add((sep2,                                                    LineStyle.Normal));
        ls.Add((now.ToString("dd/MM/yyyy  HH:mm:ss"),                   LineStyle.Normal));
        ls.Add(("",                                                       LineStyle.Normal));
        ls.Add((PadRight("Producto", 18) + PadLeft("Cant", 5) + PadLeft("Total", 9), LineStyle.Bold));
        ls.Add((sep,                                                     LineStyle.Normal));

        foreach (var item in items)
        {
            var nombre = item.Nombre.Length > 18
                ? item.Nombre[..17] + "."
                : item.Nombre;
            var linea = PadRight(nombre, 18)
                + PadLeft(item.Cantidad.ToString(), 5)
                + PadLeft($"S/{item.Precio * item.Cantidad:F2}", 9);
            ls.Add((linea, LineStyle.Normal));
            if (item.Cantidad > 1)
                ls.Add(($"  x S/{item.Precio:F2} c/u", LineStyle.Normal));
        }

        ls.Add((sep,                                                  LineStyle.Normal));
        ls.Add((PadRight("Subtotal:", 23) + PadLeft($"S/{subtotal:F2}", 9), LineStyle.Normal));
        ls.Add((PadRight("IGV (18%):", 23) + PadLeft($"S/{igv:F2}", 9),    LineStyle.Normal));
        ls.Add((PadRight("TOTAL:", 23)    + PadLeft($"S/{total:F2}", 9),   LineStyle.Bold));
        ls.Add((sep,                                                  LineStyle.Normal));
        ls.Add(($"Pago: {metodoPago}",                                LineStyle.Normal));
        ls.Add((sep2,                                                 LineStyle.Normal));
        ls.Add((Center("Gracias por su visita!"),                     LineStyle.Normal));
        ls.Add((sep2,                                                 LineStyle.Normal));
        ls.Add(("",                                                    LineStyle.Normal));
        ls.Add(("",                                                    LineStyle.Normal));

        return ls;
    }

    private static string Center(string s) =>
        s.PadLeft((32 + s.Length) / 2).PadRight(32);

    private static string PadRight(string s, int w) =>
        s.Length >= w ? s[..w] : s.PadRight(w);

    private static string PadLeft(string s, int w) =>
        s.Length >= w ? s[..w] : s.PadLeft(w);

    private enum LineStyle { Normal, Bold, Header }
}
