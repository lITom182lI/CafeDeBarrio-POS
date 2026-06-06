using CafeBarrio.POS.Data.Models;
using CafeBarrio.POS.Services;

namespace CafeBarrio.POS.Forms;

public class FormPOS : Form
{
    // Servicios
    private readonly PosService _pos;
    private readonly SyncService _sync;

    // Carrito: (ProductoId, Nombre, Precio, Cantidad)
    private readonly List<(int ProductoId, string Nombre, decimal Precio, int Cantidad)> _cart = [];

    // Controles — panel izquierdo (productos)
    private Panel _leftPanel = null!;
    private FlowLayoutPanel _productsFlow = null!;

    // Controles — panel derecho (carrito)
    private Panel _rightPanel = null!;
    private ListView _cartView = null!;
    private Label _lblSubtotalVal = null!, _lblIgvVal = null!, _lblTotalVal = null!;
    private ComboBox _cmbMetodoPago = null!;
    private Button _btnCobrar = null!, _btnLimpiar = null!;

    // Barra de estado
    private StatusStrip _statusStrip = null!;
    private ToolStripStatusLabel _lblApi = null!, _lblPendientes = null!;

    public FormPOS(PosService pos, SyncService sync)
    {
        _pos = pos;
        _sync = sync;
        InitializeForm();
        _sync.OnSyncCompleted += RefreshStatus;
    }

    // ─── Inicialización de controles ───────────────────────────────────

    private void InitializeForm()
    {
        Text = "Café de Barrio — POS";
        Size = new Size(1000, 650);
        MinimumSize = new Size(800, 550);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9.5f);

        BuildLeftPanel();
        BuildRightPanel();
        BuildStatusStrip();

        Controls.Add(_leftPanel);
        Controls.Add(_rightPanel);
        Controls.Add(_statusStrip);

        _leftPanel.Dock = DockStyle.Left;
        _leftPanel.Width = 420;
        _rightPanel.Dock = DockStyle.Fill;

        Load += async (_, _) => await OnLoadAsync();
    }

    private void BuildLeftPanel()
    {
        _leftPanel = new Panel { BackColor = Color.FromArgb(250, 250, 250), Padding = new Padding(12) };

        var header = new Label
        {
            Text = "PRODUCTOS",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 100, 100),
            AutoSize = true,
            Location = new Point(12, 12)
        };

        _productsFlow = new FlowLayoutPanel
        {
            Location = new Point(0, 40),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            AutoScroll = true,
            Padding = new Padding(8),
            BackColor = Color.FromArgb(250, 250, 250)
        };

        _leftPanel.Controls.Add(header);
        _leftPanel.Controls.Add(_productsFlow);

        _leftPanel.Resize += (_, _) =>
        {
            _productsFlow.Size = new Size(
                _leftPanel.Width,
                _leftPanel.Height - 40);
        };
    }

    private void BuildRightPanel()
    {
        _rightPanel = new Panel { Padding = new Padding(12) };

        var headerCart = new Label
        {
            Text = "CARRITO",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 100, 100),
            AutoSize = true,
            Location = new Point(12, 12)
        };

        _cartView = new ListView
        {
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Location = new Point(12, 38),
            Size = new Size(_rightPanel.Width - 24, 300),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };
        _cartView.Columns.Add("Producto", 180);
        _cartView.Columns.Add("Cant.", 55);
        _cartView.Columns.Add("Precio", 80);
        _cartView.Columns.Add("Subtotal", 90);

        // Clic derecho en carrito → eliminar item
        var ctxMenu = new ContextMenuStrip();
        var removeItem = new ToolStripMenuItem("Quitar del carrito");
        removeItem.Click += (_, _) =>
        {
            if (_cartView.SelectedIndices.Count > 0)
            {
                _cart.RemoveAt(_cartView.SelectedIndices[0]);
                RefreshCart();
            }
        };
        ctxMenu.Items.Add(removeItem);
        _cartView.ContextMenuStrip = ctxMenu;

        // Totales
        int y = 350;
        _lblSubtotalVal = MakeInfoLabel("", new Point(_rightPanel.Width - 120, y));
        var lblSubtotalTxt = MakeInfoLabel("Subtotal:", new Point(12, y)); y += 28;
        _lblIgvVal = MakeInfoLabel("", new Point(_rightPanel.Width - 120, y));
        var lblIgvTxt = MakeInfoLabel("IGV (18%):", new Point(12, y)); y += 28;
        _lblTotalVal = MakeInfoLabel("", new Point(_rightPanel.Width - 120, y));
        _lblTotalVal.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
        _lblTotalVal.ForeColor = Color.FromArgb(194, 98, 42);
        var lblTotalTxt = MakeInfoLabel("TOTAL:", new Point(12, y));
        lblTotalTxt.Font = new Font("Segoe UI", 11f, FontStyle.Bold); y += 36;

        // Método de pago
        var lblMetodo = new Label { Text = "Método de pago:", AutoSize = true, Location = new Point(12, y) }; y += 22;
        _cmbMetodoPago = new ComboBox
        {
            Location = new Point(12, y),
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList
        }; y += 36;

        // Botones
        _btnLimpiar = new Button
        {
            Text = "Limpiar",
            Location = new Point(12, y),
            Size = new Size(90, 38),
            BackColor = Color.FromArgb(240, 240, 240),
            FlatStyle = FlatStyle.Flat
        };
        _btnCobrar = new Button
        {
            Text = "COBRAR",
            Location = new Point(112, y),
            Size = new Size(140, 38),
            BackColor = Color.FromArgb(194, 98, 42),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat
        };
        _btnCobrar.FlatAppearance.BorderSize = 0;

        _btnLimpiar.Click += (_, _) => { _cart.Clear(); RefreshCart(); };
        _btnCobrar.Click += async (_, _) => await OnCobrarAsync();

        // Anclar labels de totales al lado derecho
        _rightPanel.Resize += (_, _) =>
        {
            int w = _rightPanel.Width;
            _cartView.Width = w - 24;
            int ry = 350;
            _lblSubtotalVal.Location = new Point(w - 130, ry); ry += 28;
            _lblIgvVal.Location = new Point(w - 130, ry); ry += 28;
            _lblTotalVal.Location = new Point(w - 130, ry);
        };

        _rightPanel.Controls.AddRange(new Control[] {
            headerCart, _cartView,
            lblSubtotalTxt, _lblSubtotalVal,
            lblIgvTxt, _lblIgvVal,
            lblTotalTxt, _lblTotalVal,
            lblMetodo, _cmbMetodoPago,
            _btnLimpiar, _btnCobrar
        });
    }

    private Label MakeInfoLabel(string text, Point loc) => new()
    {
        Text = text, AutoSize = true, Location = loc, Font = new Font("Segoe UI", 10f)
    };

    private void BuildStatusStrip()
    {
        _statusStrip = new StatusStrip();
        _lblApi = new ToolStripStatusLabel("Conectando...");
        _lblPendientes = new ToolStripStatusLabel("") { Spring = true, TextAlign = ContentAlignment.MiddleRight };
        _statusStrip.Items.AddRange(new ToolStripItem[] { _lblApi, _lblPendientes });
    }

    // ─── Carga inicial ─────────────────────────────────────────────────

    private async Task OnLoadAsync()
    {
        _btnCobrar.Enabled = false;
        _lblApi.Text = "Conectando con API...";
        await _pos.CargarCatalogoAsync();
        RenderProductButtons();
        LoadMetodosPago();
        RefreshCart();
        RefreshStatus();
        _btnCobrar.Enabled = true;
        _sync.Start();
    }

    private void RenderProductButtons()
    {
        _productsFlow.Controls.Clear();
        foreach (var p in _pos.Productos)
        {
            var btn = new Button
            {
                Text = $"{p.Nombre}\nS/ {p.Precio:F2}",
                Size = new Size(130, 65),
                Margin = new Padding(5),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter,
                Tag = p
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
            btn.MouseEnter += (_, _) => btn.BackColor = Color.FromArgb(255, 243, 235);
            btn.MouseLeave += (_, _) => btn.BackColor = Color.White;
            btn.Click += (_, _) => AgregarAlCarrito(p);
            _productsFlow.Controls.Add(btn);
        }
    }

    private void LoadMetodosPago()
    {
        _cmbMetodoPago.Items.Clear();
        foreach (var m in _pos.MetodosPago)
            _cmbMetodoPago.Items.Add(m);
        _cmbMetodoPago.DisplayMember = "Nombre";
        if (_cmbMetodoPago.Items.Count > 0) _cmbMetodoPago.SelectedIndex = 0;
    }

    // ─── Carrito ───────────────────────────────────────────────────────

    private void AgregarAlCarrito(CachedProducto p)
    {
        var idx = _cart.FindIndex(c => c.ProductoId == p.ProductoId);
        if (idx >= 0)
        {
            var item = _cart[idx];
            _cart[idx] = item with { Cantidad = item.Cantidad + 1 };
        }
        else
        {
            _cart.Add((p.ProductoId, p.Nombre, p.Precio, 1));
        }
        RefreshCart();
    }

    private void RefreshCart()
    {
        _cartView.Items.Clear();
        foreach (var item in _cart)
        {
            var row = new ListViewItem(item.Nombre);
            row.SubItems.Add(item.Cantidad.ToString());
            row.SubItems.Add($"S/ {item.Precio:F2}");
            row.SubItems.Add($"S/ {item.Precio * item.Cantidad:F2}");
            _cartView.Items.Add(row);
        }

        var (sub, igv, total) = _pos.CalcularTotales(_cart);
        _lblSubtotalVal.Text = $"S/ {sub:F2}";
        _lblIgvVal.Text = $"S/ {igv:F2}";
        _lblTotalVal.Text = $"S/ {total:F2}";
    }

    // ─── Cobrar ────────────────────────────────────────────────────────

    private async Task OnCobrarAsync()
    {
        if (_cart.Count == 0)
        {
            MessageBox.Show("Agrega al menos un producto al carrito.",
                "Carrito vacío", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (_cmbMetodoPago.SelectedItem is not CachedMetodoPago metodo)
        {
            MessageBox.Show("Selecciona un método de pago.",
                "Pago requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _btnCobrar.Enabled = false;
        _btnCobrar.Text = "Procesando...";

        var sincronizado = await _pos.RegistrarVentaAsync(metodo.MetodoPagoId, _cart);
        _cart.Clear();
        RefreshCart();
        RefreshStatus();

        var msg = sincronizado
            ? "✓ Venta registrada y sincronizada con el servidor."
            : "✓ Venta guardada localmente.\nSe sincronizará cuando la API esté disponible.";
        var icon = sincronizado ? MessageBoxIcon.Information : MessageBoxIcon.Warning;
        MessageBox.Show(msg, "Venta completada", MessageBoxButtons.OK, icon);

        _btnCobrar.Enabled = true;
        _btnCobrar.Text = "COBRAR";
    }

    // ─── Estado ────────────────────────────────────────────────────────

    private void RefreshStatus()
    {
        if (InvokeRequired) { Invoke(RefreshStatus); return; }

        _ = Task.Run(async () =>
        {
            var disponible = await _pos_api_check();
            var pendientes = _pos.GetPendienteCount();
            Invoke(() =>
            {
                _lblApi.Text = disponible ? "● API conectada" : "○ Sin conexión (offline)";
                _lblApi.ForeColor = disponible ? Color.DarkGreen : Color.OrangeRed;
                _lblPendientes.Text = pendientes > 0
                    ? $"Pendientes de sync: {pendientes}"
                    : "Todo sincronizado ✓";
            });
        });
    }

    private Func<Task<bool>> _pos_api_check = () => Task.FromResult(false);

    public void SetApiCheck(Func<Task<bool>> check) => _pos_api_check = check;

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _sync.Dispose();
        base.OnFormClosed(e);
    }
}
