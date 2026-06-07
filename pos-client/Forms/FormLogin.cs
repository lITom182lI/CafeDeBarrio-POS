using CafeBarrio.POS.Services;

namespace CafeBarrio.POS.Forms;

public class FormLogin : Form
{
    private readonly ApiClient _api;
    private ComboBox _cmbOperador  = null!;
    private TextBox  _txtPin       = null!;
    private Label    _lblError     = null!;
    private Button   _btnIngresar  = null!;
    private Button   _btnAnonimo   = null!;

    public int? OperadorId { get; private set; }

    public FormLogin(ApiClient api)
    {
        _api = api;
        BuildUI();
        Load += async (_, _) => await LoadOperadoresAsync();
    }

    private void BuildUI()
    {
        Text            = "Café de Barrio — Acceso";
        Size            = new Size(320, 295);
        MinimumSize     = Size;
        MaximumSize     = Size;
        StartPosition   = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        AutoScaleMode   = AutoScaleMode.None;
        BackColor       = Color.White;
        Font            = new Font("Segoe UI", 9.5f);

        var lblTitulo = new Label
        {
            Text      = "Café de Barrio",
            Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = Color.FromArgb(194, 98, 42),
            AutoSize  = true,
            Location  = new Point(78, 18)
        };

        var lblCajero = new Label
        {
            Text     = "Cajero:",
            AutoSize = true,
            Location = new Point(50, 60)
        };

        _cmbOperador = new ComboBox
        {
            Location      = new Point(50, 78),
            Width         = 210,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Enabled       = false
        };

        var lblPin = new Label
        {
            Text     = "PIN:",
            AutoSize = true,
            Location = new Point(50, 118)
        };

        _txtPin = new TextBox
        {
            Location     = new Point(50, 136),
            Width        = 210,
            MaxLength    = 8,
            PasswordChar = '●',
            Font         = new Font("Segoe UI", 12f),
            TextAlign    = HorizontalAlignment.Center,
            Enabled      = false
        };
        _txtPin.KeyPress += (_, e) =>
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
                e.Handled = true;
        };
        _txtPin.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) _btnIngresar.PerformClick();
        };

        _btnIngresar = new Button
        {
            Text      = "INGRESAR",
            Location  = new Point(65, 178),
            Size      = new Size(180, 36),
            BackColor = Color.FromArgb(194, 98, 42),
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Enabled   = false
        };
        _btnIngresar.FlatAppearance.BorderSize = 0;
        _btnIngresar.Click += async (_, _) => await OnIngresarAsync();

        _btnAnonimo = new Button
        {
            Text      = "Continuar sin identificar",
            Location  = new Point(65, 178),
            Size      = new Size(180, 36),
            BackColor = Color.FromArgb(220, 220, 220),
            FlatStyle = FlatStyle.Flat,
            Visible   = false
        };
        _btnAnonimo.Click += (_, _) =>
        {
            OperadorId   = null;
            DialogResult = DialogResult.OK;
            Close();
        };

        _lblError = new Label
        {
            Text      = "Cargando operadores...",
            AutoSize  = false,
            Size      = new Size(220, 40),
            ForeColor = Color.FromArgb(100, 100, 100),
            Location  = new Point(50, 225),
            TextAlign = ContentAlignment.TopLeft
        };

        Controls.AddRange(new Control[]
            { lblTitulo, lblCajero, _cmbOperador, lblPin, _txtPin,
              _btnIngresar, _btnAnonimo, _lblError });
    }

    private async Task LoadOperadoresAsync()
    {
        var operadores = await _api.GetOperadoresAsync();

        if (operadores.Count == 0)
        {
            _lblError.Text      = "Sin conexión — no se pudo cargar la lista de cajeros.";
            _lblError.ForeColor = Color.OrangeRed;
            _btnAnonimo.Visible = true;
            return;
        }

        foreach (var op in operadores)
            _cmbOperador.Items.Add(op);

        _cmbOperador.DisplayMember = "Nombre";
        _cmbOperador.SelectedIndex = 0;
        _cmbOperador.Enabled       = true;
        _txtPin.Enabled            = true;
        _btnIngresar.Enabled       = true;
        _lblError.Text             = "";
        _txtPin.Focus();
    }

    private async Task OnIngresarAsync()
    {
        if (_cmbOperador.SelectedItem is not OperadorDto operador)
        {
            _lblError.Text = "Seleccione un cajero.";
            return;
        }

        var pin = _txtPin.Text.Trim();
        if (pin.Length < 4)
        {
            _lblError.Text = "El PIN debe tener al menos 4 dígitos.";
            return;
        }

        _btnIngresar.Enabled = false;
        _btnIngresar.Text    = "Verificando...";
        _lblError.Text       = "";

        try
        {
            var resultado = await _api.ValidarPinAsync(operador.OperadorId, pin);
            if (resultado is not null)
            {
                OperadorId   = resultado.OperadorId;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                _lblError.ForeColor = Color.Crimson;
                _lblError.Text      = "PIN incorrecto. Intente nuevamente.";
                _txtPin.Clear();
                _txtPin.Focus();
            }
        }
        catch
        {
            _lblError.ForeColor = Color.OrangeRed;
            _lblError.Text      = "Error de conexión. Intente nuevamente.";
        }
        finally
        {
            if (!IsDisposed)
            {
                _btnIngresar.Enabled = true;
                _btnIngresar.Text    = "INGRESAR";
            }
        }
    }
}
