using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace CodeAppsDataMigration
{
    /// <summary>Who signed in on the start-up login form.</summary>
    public static class AppSession
    {
        public static string SuperUserName { get; set; } = "";
        public static string SuperUserToken { get; set; } = "";
    }

    /// <summary>
    /// Super user sign in shown when the app starts (same flow as the web app's
    /// routes/sessions/login: POST api/SuperUser/Login, and on an older API without it (404)
    /// the SuperUserList check). The API base comes from ConnectionStrings.xml → ApiUrls/MainBranchUrl
    /// (everything up to and including "/api/").
    /// </summary>
    public class SuperUserLoginForm : Form
    {
        private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(30) };

        private static readonly Color Accent = Color.FromArgb(32, 169, 243);
        private static readonly Color AccentDark = Color.FromArgb(18, 132, 199);
        private static readonly Color Ink = Color.FromArgb(30, 41, 59);
        private static readonly Color Muted = Color.FromArgb(100, 116, 139);
        private static readonly Color FieldBorder = Color.FromArgb(203, 213, 225);
        private static readonly Color PanelBack = Color.FromArgb(244, 247, 251);

        private static string XmlPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConnectionStrings.xml");
        private static string RememberPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CodeAppsDataMigration", "login.json");

        private TextBox txtUsername = null!;
        private TextBox txtPassword = null!;
        private TextBox txtServer = null!;
        private CheckBox chkRemember = null!;
        private Button btnSignIn = null!;
        private Button btnConnectionSettings = null!;
        private Label lblStatus = null!;
        private ProgressBar prgLoading = null!;
        private Panel pnlServer = null!;
        private LinkLabel lnkServer = null!;
        private bool _isLoading;

        public SuperUserLoginForm()
        {
            InitializeControls();
            txtServer.Text = ReadApiBaseUrl();
            UpdateServerLink();
            LoadRemembered();
        }

        // ──────────────────────────────────────────────────────────────
        //  Layout
        // ──────────────────────────────────────────────────────────────

        private void InitializeControls()
        {
            Text = "CODEAPPS Data Migration - Sign In";
            ClientSize = new Size(980, 600);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = PanelBack;
            Font = new Font("Segoe UI", 9.5F);
            KeyPreview = true;
            KeyDown += (_, e) => { if (e.KeyCode == Keys.Escape && !_isLoading) Close(); };

            var showcase = BuildShowcase();
            showcase.Dock = DockStyle.Left;
            showcase.Width = 470;

            var formPanel = new Panel { Dock = DockStyle.Fill, BackColor = PanelBack };
            var card = BuildCard();
            formPanel.Controls.Add(card);
            formPanel.Resize += (_, _) => card.Location = new Point(
                (formPanel.ClientSize.Width - card.Width) / 2, (formPanel.ClientSize.Height - card.Height) / 2);

            Controls.Add(formPanel);
            Controls.Add(showcase);
            Shown += (_, _) => (string.IsNullOrEmpty(txtUsername.Text) ? txtUsername : txtPassword).Focus();
        }

        private Panel BuildShowcase()
        {
            var p = new DoubleBufferedPanel();
            p.Paint += (_, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = p.ClientRectangle;
                using (var bg = new LinearGradientBrush(r, Color.FromArgb(11, 23, 48), Color.FromArgb(17, 64, 112), 55f))
                    g.FillRectangle(bg, r);
                DrawGlow(g, new Rectangle(-120, -110, 360, 360), Color.FromArgb(32, 169, 243));
                DrawGlow(g, new Rectangle(r.Width - 220, r.Height / 2 - 60, 320, 320), Color.FromArgb(124, 58, 237));
                DrawGlow(g, new Rectangle(60, r.Height - 160, 260, 260), Color.FromArgb(16, 185, 129));
            };

            // Brand
            var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "codeapps.png");
            var logo = new PictureBox
            {
                Size = new Size(40, 40),
                Location = new Point(48, 44),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            if (File.Exists(logoPath)) logo.Image = Image.FromFile(logoPath);
            p.Controls.Add(logo);
            p.Controls.Add(TransparentLabel("CODEAPPS", new Font("Segoe UI", 15F, FontStyle.Bold), Color.White, new Point(96, 49)));

            // Pitch
            p.Controls.Add(TransparentLabel("✦  Data migration suite", new Font("Segoe UI", 9F, FontStyle.Bold),
                Color.FromArgb(147, 214, 251), new Point(48, 140)));
            p.Controls.Add(TransparentLabel("Move your data", new Font("Segoe UI", 26F, FontStyle.Bold), Color.White, new Point(44, 168)));
            p.Controls.Add(TransparentLabel("without limits.", new Font("Segoe UI", 26F, FontStyle.Bold), Accent, new Point(44, 214)));
            var sub = TransparentLabel(
                "SQL Server, offline and online PostgreSQL — migrated, verified and compared in one place.",
                new Font("Segoe UI", 10.5F), Color.FromArgb(203, 213, 225), new Point(48, 276));
            sub.AutoSize = false;
            sub.Size = new Size(380, 48);
            p.Controls.Add(sub);

            // Features
            var features = new[]
            {
                ("⇄", "SQL Server → PostgreSQL", "Table-by-table migration with FK fix-ups"),
                ("☁", "Offline → Online", "Push branches and data to the online server"),
                ("✓", "Count compare", "Check every table landed with the right rows"),
            };
            int y = 350;
            foreach (var (icon, title, text) in features)
            {
                var badge = new RoundedPanel
                {
                    Size = new Size(38, 38),
                    Location = new Point(48, y),
                    FillColor = Color.FromArgb(40, 255, 255, 255),
                    BorderColor = Color.FromArgb(60, 255, 255, 255),
                    Radius = 10,
                    BackColor = Color.Transparent
                };
                var ic = new Label
                {
                    Text = icon,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI Symbol", 13F),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent
                };
                badge.Controls.Add(ic);
                p.Controls.Add(badge);
                p.Controls.Add(TransparentLabel(title, new Font("Segoe UI", 10.5F, FontStyle.Bold), Color.White, new Point(100, y)));
                p.Controls.Add(TransparentLabel(text, new Font("Segoe UI", 9F), Color.FromArgb(160, 180, 205), new Point(100, y + 20)));
                y += 58;
            }

            p.Controls.Add(TransparentLabel("© 2025 CODEAPPS Technology Private Limited", new Font("Segoe UI", 8.5F),
                Color.FromArgb(120, 140, 170), new Point(48, 556)));
            return p;
        }

        private Panel BuildCard()
        {
            var card = new RoundedPanel
            {
                Size = new Size(410, 520),
                FillColor = Color.White,
                BorderColor = Color.FromArgb(226, 232, 240),
                Radius = 16,
                BackColor = PanelBack
            };

            const int left = 36, width = 338;

            var chip = new RoundedPanel
            {
                Location = new Point(left, 32),
                Size = new Size(118, 26),
                FillColor = Color.FromArgb(224, 242, 254),
                BorderColor = Color.FromArgb(224, 242, 254),
                Radius = 13,
                BackColor = Color.White
            };
            chip.Controls.Add(new Label
            {
                Text = "★  SUPER USER",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = AccentDark,
                BackColor = Color.Transparent
            });
            card.Controls.Add(chip);

            card.Controls.Add(new Label
            {
                Text = "Welcome back",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Ink,
                AutoSize = true,
                Location = new Point(left - 3, 68),
                BackColor = Color.White
            });
            card.Controls.Add(new Label
            {
                Text = "Sign in with your super user account to continue.",
                ForeColor = Muted,
                AutoSize = true,
                Location = new Point(left, 106),
                BackColor = Color.White
            });

            // Username
            card.Controls.Add(FieldLabel("Username", new Point(left, 144)));
            txtUsername = new TextBox { PlaceholderText = "Enter your username" };
            card.Controls.Add(FieldBox(txtUsername, new Point(left, 166), width, "👤", null));
            txtUsername.KeyDown += (_, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                txtPassword.Focus();
            };

            // Password
            card.Controls.Add(FieldLabel("Password", new Point(left, 218)));
            txtPassword = new TextBox { PlaceholderText = "Enter your password", UseSystemPasswordChar = true };
            var eye = new Label
            {
                Text = "👁",
                Font = new Font("Segoe UI Emoji", 10F),
                ForeColor = Muted,
                Cursor = Cursors.Hand,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter
            };
            eye.Click += (_, _) =>
            {
                txtPassword.UseSystemPasswordChar = !txtPassword.UseSystemPasswordChar;
                eye.ForeColor = txtPassword.UseSystemPasswordChar ? Muted : Accent;
            };
            card.Controls.Add(FieldBox(txtPassword, new Point(left, 240), width, null, eye));
            txtPassword.KeyDown += async (_, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                await SignInAsync();
            };

            // Options
            chkRemember = new CheckBox
            {
                Text = "Remember me",
                Checked = true,
                AutoSize = true,
                ForeColor = Ink,
                Location = new Point(left, 296),
                BackColor = Color.White
            };
            card.Controls.Add(chkRemember);
            var secure = new Label
            {
                Text = "🔒 Secure session",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Muted,
                AutoSize = true,
                BackColor = Color.White
            };
            card.Controls.Add(secure);
            secure.Location = new Point(left + width - secure.PreferredWidth, 299);

            // Sign in
            btnSignIn = new Button
            {
                Text = "Sign In   →",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Accent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(width, 44),
                Location = new Point(left, 334),
                Cursor = Cursors.Hand
            };
            btnSignIn.FlatAppearance.BorderSize = 0;
            btnSignIn.FlatAppearance.MouseOverBackColor = AccentDark;
            btnSignIn.Click += async (_, _) => await SignInAsync();
            card.Controls.Add(btnSignIn);

            prgLoading = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 25,
                Size = new Size(width, 4),
                Location = new Point(left, 380),
                Visible = false
            };
            card.Controls.Add(prgLoading);

            lblStatus = new Label
            {
                AutoSize = false,
                Size = new Size(width, 20),
                Location = new Point(left, 388),
                ForeColor = Color.FromArgb(220, 38, 38),
                BackColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(lblStatus);

            // Connection settings (same dialog as the Dashboard's)
            btnConnectionSettings = new Button
            {
                Text = "⚙   Connection Settings",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = AccentDark,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(width, 40),
                Location = new Point(left, 414),
                Cursor = Cursors.Hand
            };
            btnConnectionSettings.FlatAppearance.BorderColor = Color.FromArgb(186, 230, 253);
            btnConnectionSettings.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 249, 255);
            btnConnectionSettings.Click += (_, _) => OpenConnectionSettings();
            card.Controls.Add(btnConnectionSettings);

            // API server (collapsed unless changed)
            lnkServer = new LinkLabel
            {
                AutoSize = false,
                Size = new Size(width, 20),
                Location = new Point(left, 466),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8.5F),
                LinkColor = Muted,
                ActiveLinkColor = Accent,
                LinkBehavior = LinkBehavior.HoverUnderline,
                BackColor = Color.White
            };
            lnkServer.LinkClicked += (_, _) =>
            {
                pnlServer.Visible = !pnlServer.Visible;
                lnkServer.Visible = !pnlServer.Visible;
                if (pnlServer.Visible) txtServer.Focus();
            };
            card.Controls.Add(lnkServer);

            pnlServer = new Panel { Location = new Point(left, 460),Size = new Size(width, 34), Visible = false, BackColor = Color.White };
            txtServer = new TextBox { Location = new Point(0, 5), Width = width - 70, Font = new Font("Segoe UI", 9F) };
            var btnServerOk = new Button
            {
                Text = "Save",
                Location = new Point(width - 64, 3),
                Size = new Size(64, 28),
                FlatStyle = FlatStyle.Flat,
                ForeColor = AccentDark,
                BackColor = Color.FromArgb(224, 242, 254),
                Cursor = Cursors.Hand
            };
            btnServerOk.FlatAppearance.BorderSize = 0;
            btnServerOk.Click += (_, _) => SaveServer();
            txtServer.KeyDown += (_, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                SaveServer();
            };
            pnlServer.Controls.Add(txtServer);
            pnlServer.Controls.Add(btnServerOk);
            card.Controls.Add(pnlServer);

            return card;
        }

        private static Label FieldLabel(string text, Point location) => new()
        {
            Text = text,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Ink,
            AutoSize = true,
            Location = location,
            BackColor = Color.White
        };

        /// <summary>Borderless textbox inside a rounded outline that turns accent on focus.</summary>
        private static Control FieldBox(TextBox box, Point location, int width, string? icon, Label? trailing)
        {
            var outline = new RoundedPanel
            {
                Location = location,
                Size = new Size(width, 42),
                FillColor = Color.White,
                BorderColor = FieldBorder,
                Radius = 8,
                BackColor = Color.White
            };
            box.BorderStyle = BorderStyle.None;
            box.Font = new Font("Segoe UI", 10.5F);
            box.ForeColor = Ink;
            box.Location = new Point(14, 11);
            box.Width = width - 52;
            outline.Controls.Add(box);

            var suffix = trailing ?? new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 10F),
                ForeColor = Muted,
                TextAlign = ContentAlignment.MiddleCenter
            };
            suffix.Size = new Size(30, 30);
            suffix.Location = new Point(width - 38, 6);
            suffix.BackColor = Color.White;
            outline.Controls.Add(suffix);

            box.Enter += (_, _) => { outline.BorderColor = Accent; outline.BorderWidth = 2; outline.Invalidate(); };
            box.Leave += (_, _) => { outline.BorderColor = FieldBorder; outline.BorderWidth = 1; outline.Invalidate(); };
            outline.Click += (_, _) => box.Focus();
            return outline;
        }

        private static Label TransparentLabel(string text, Font font, Color color, Point location) => new()
        {
            Text = text,
            Font = font,
            ForeColor = color,
            AutoSize = true,
            Location = location,
            BackColor = Color.Transparent
        };

        private static void DrawGlow(Graphics g, Rectangle r, Color c)
        {
            using var path = new GraphicsPath();
            path.AddEllipse(r);
            using var brush = new PathGradientBrush(path)
            {
                CenterColor = Color.FromArgb(70, c),
                SurroundColors = new[] { Color.FromArgb(0, c) }
            };
            g.FillEllipse(brush, r);
        }

        // ──────────────────────────────────────────────────────────────
        //  Sign in
        // ──────────────────────────────────────────────────────────────

        private async Task SignInAsync()
        {
            if (_isLoading) return;

            var username = txtUsername.Text.Trim();
            var password = txtPassword.Text;   // do NOT trim password
            if (username.Length == 0 || password.Length == 0)
            {
                ShowError("Please enter username and password");
                (username.Length == 0 ? txtUsername : txtPassword).Focus();
                return;
            }

            var apiBase = NormalizeBase(txtServer.Text);
            if (apiBase.Length == 0)
            {
                ShowError("Set the API server first");
                pnlServer.Visible = true;
                lnkServer.Visible = false;
                txtServer.Focus();
                return;
            }

            SetLoading(true, "Verifying credentials...");
            try
            {
                var (ok, token, message) = await LoginAsync(apiBase, username, password);
                if (!ok)
                {
                    SetLoading(false);
                    ShowError(message);
                    txtPassword.SelectAll();
                    txtPassword.Focus();
                    return;
                }

                AppSession.SuperUserName = username;
                AppSession.SuperUserToken = token;
                SaveRemembered(username);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (TaskCanceledException)
            {
                SetLoading(false);
                ShowError("The server did not answer. Check the API server.");
            }
            catch (HttpRequestException ex)
            {
                SetLoading(false);
                ShowError("Cannot reach the API server: " + ex.Message);
            }
            catch (Exception ex)
            {
                SetLoading(false);
                ShowError("Login failed. " + ex.Message);
            }
        }

        /// <summary>
        /// POST SuperUser/Login { username, pwd } → { Flag, Message, Content: { Token, username } }.
        /// An API older than SuperUser/Login (404) is checked the old way through SuperUserList.
        /// </summary>
        private static async Task<(bool Ok, string Token, string Message)> LoginAsync(string apiBase, string username, string password)
        {
            var (status, root) = await PostAsync(apiBase + "SuperUser/Login", username, password);
            if (status == HttpStatusCode.NotFound)
                return await LoginLegacyAsync(apiBase, username, password);
            if (root is null)
                return (false, "", $"Login failed ({(int)status}).");

            var r = root.Value;
            if (GetBool(r, "Flag") && TryGet(r, "Content", out var content)
                && TryGet(content, "Token", out var tok) && tok.ValueKind == JsonValueKind.String)
                return (true, tok.GetString() ?? "", "");

            return (false, "", GetString(r, "Message") is { Length: > 0 } m ? m : "Invalid Username & Password");
        }

        private static async Task<(bool Ok, string Token, string Message)> LoginLegacyAsync(string apiBase, string username, string password)
        {
            var (status, root) = await PostAsync(apiBase + "SuperUser/SuperUserList", username, password);
            if (root is null || !GetBool(root.Value, "Flag"))
                return (false, "", root is null ? $"Login failed ({(int)status})." : "Invalid Username & Password");

            if (!TryGet(root.Value, "Content", out var content))
                return (false, "", "Invalid Username & Password");

            var users = content.ValueKind == JsonValueKind.Array ? content.EnumerateArray().ToList() : new List<JsonElement> { content };
            bool match = users.Any(u =>
                string.Equals(GetString(u, "username"), username, StringComparison.OrdinalIgnoreCase)
                && GetString(u, "pwd") == password);

            return match ? (true, "", "") : (false, "", "Invalid Username & Password");
        }

        private static async Task<(HttpStatusCode Status, JsonElement? Root)> PostAsync(string url, string username, string password)
        {
            var json = JsonSerializer.Serialize(new { username, pwd = password });
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            var apiKey = ReadApiUrlsValue("ApiKey");
            if (!string.IsNullOrEmpty(apiKey))
                request.Headers.Add("X-Api-Key", apiKey);

            using var resp = await _httpClient.SendAsync(request);
            var body = await resp.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body)) return (resp.StatusCode, null);
            try
            {
                using var doc = JsonDocument.Parse(body);
                return (resp.StatusCode, doc.RootElement.Clone());
            }
            catch (JsonException)
            {
                return (resp.StatusCode, null);
            }
        }

        // Property names are matched case-insensitively (the API may send Flag or flag).
        private static bool TryGet(JsonElement e, string name, out JsonElement value)
        {
            value = default;
            if (e.ValueKind != JsonValueKind.Object) return false;
            foreach (var p in e.EnumerateObject())
                if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = p.Value;
                    return true;
                }
            return false;
        }

        private static bool GetBool(JsonElement e, string name) =>
            TryGet(e, name, out var v) && v.ValueKind == JsonValueKind.True;

        private static string GetString(JsonElement e, string name) =>
            TryGet(e, name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";

        private void SetLoading(bool loading, string message = "")
        {
            _isLoading = loading;
            btnSignIn.Enabled = !loading;
            btnSignIn.Text = loading ? "Signing in…" : "Sign In   →";
            btnSignIn.BackColor = loading ? Color.FromArgb(125, 200, 245) : Accent;
            txtUsername.Enabled = txtPassword.Enabled = btnConnectionSettings.Enabled = !loading;
            prgLoading.Visible = loading;
            lblStatus.ForeColor = Muted;
            lblStatus.Text = message;
            UseWaitCursor = loading;
        }

        private void ShowError(string message)
        {
            lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
            lblStatus.Text = message;
        }

        // ──────────────────────────────────────────────────────────────
        //  API server + remember me
        // ──────────────────────────────────────────────────────────────

        /// <summary>ApiUrls/MainBranchUrl (saved by ConnectionSettingsForm) cut after "/api/".</summary>
        private static string ReadApiBaseUrl()
        {
            var mainUrl = ReadApiUrlsValue("MainBranchUrl");
            var i = mainUrl.IndexOf("/api/", StringComparison.OrdinalIgnoreCase);
            return i >= 0 ? mainUrl[..(i + 5)] : "";
        }

        private static string ReadApiUrlsValue(string name)
        {
            if (!File.Exists(XmlPath)) return "";
            var doc = XDocument.Load(XmlPath);
            return (doc.Root?.Element("ApiUrls")?.Element(name)?.Value ?? "").Trim();
        }

        private static string NormalizeBase(string url)
        {
            url = url.Trim();
            if (url.Length == 0) return "";
            return url.EndsWith('/') ? url : url + "/";
        }

        private void SaveServer()
        {
            var url = NormalizeBase(txtServer.Text);
            if (!Uri.TryCreate(url, UriKind.Absolute, out _)
                || !url.EndsWith("/api/", StringComparison.OrdinalIgnoreCase))
            {
                ShowError("Enter a full URL ending in /api/, e.g. https://localhost:44301/api/");
                return;
            }
            txtServer.Text = url;
            try
            {
                if (File.Exists(XmlPath))
                {
                    var doc = XDocument.Load(XmlPath);
                    var apiUrls = doc.Root!.Element("ApiUrls");
                    if (apiUrls == null)
                    {
                        apiUrls = new XElement("ApiUrls");
                        doc.Root.Add(apiUrls);
                    }
                    // Same endpoints as ConnectionSettingsForm's Environment presets.
                    apiUrls.SetElementValue("MainBranchUrl", url + "mainbranch/CreateMainBranch");
                    apiUrls.SetElementValue("SubBranchUrl", url + "branch/CreateBranch");
                    doc.Save(XmlPath);
                }
            }
            catch (Exception ex)
            {
                ShowError("Could not save the server: " + ex.Message);
                return;
            }
            lblStatus.Text = "";
            pnlServer.Visible = false;
            lnkServer.Visible = true;
            UpdateServerLink();
            txtUsername.Focus();
        }

        private void OpenConnectionSettings()
        {
            if (_isLoading) return;
            using (var form = new ConnectionSettingsForm())
                form.ShowDialog(this);

            // The API URLs may have changed there.
            txtServer.Text = ReadApiBaseUrl();
            UpdateServerLink();
            txtUsername.Focus();
        }

        private void UpdateServerLink()
        {
            var url = NormalizeBase(txtServer.Text);
            lnkServer.Text = url.Length == 0 ? "⚙  Set API server" : $"⚙  {url}  ·  change";
        }

        // Only the username is remembered; the password is never written to disk.
        private void LoadRemembered()
        {
            try
            {
                if (!File.Exists(RememberPath)) return;
                using var doc = JsonDocument.Parse(File.ReadAllText(RememberPath));
                txtUsername.Text = GetString(doc.RootElement, "username");
                chkRemember.Checked = true;
            }
            catch
            {
                // Corrupted file — ignore it.
            }
        }

        private void SaveRemembered(string username)
        {
            try
            {
                if (!chkRemember.Checked)
                {
                    if (File.Exists(RememberPath)) File.Delete(RememberPath);
                    return;
                }
                Directory.CreateDirectory(Path.GetDirectoryName(RememberPath)!);
                File.WriteAllText(RememberPath, JsonSerializer.Serialize(new { username }));
            }
            catch
            {
                // Remember me is a convenience; never block the sign in over it.
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Small drawing helpers
        // ──────────────────────────────────────────────────────────────

        private class DoubleBufferedPanel : Panel
        {
            public DoubleBufferedPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
            }
        }

        private class RoundedPanel : Panel
        {
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public Color FillColor { get; set; } = Color.White;
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public Color BorderColor { get; set; } = Color.Gainsboro;
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public int BorderWidth { get; set; } = 1;
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public int Radius { get; set; } = 8;

            public RoundedPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
                SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, Width - 1, Height - 1);
                using var path = RoundRect(r, Radius);
                using (var fill = new SolidBrush(FillColor)) g.FillPath(fill, path);
                using var pen = new Pen(BorderColor, BorderWidth) { Alignment = PenAlignment.Inset };
                g.DrawPath(pen, path);
            }

            private static GraphicsPath RoundRect(Rectangle r, int radius)
            {
                int d = radius * 2;
                var path = new GraphicsPath();
                path.AddArc(r.X, r.Y, d, d, 180, 90);
                path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                return path;
            }
        }
    }
}
