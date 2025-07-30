using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Media;

namespace SymlinkCreator
{
    public static class CustomMessageBox
    {
        public static DialogResult Show(
            string text,
            string caption = "���b�Z�[�W",
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.None,
            int padding = 20)
        {
            using (var form = new Form())
            {
                Screen? primaryScreen = Screen.PrimaryScreen ?? throw new NotSupportedException("Primary screen is not found");
                int screenWidth = primaryScreen.WorkingArea.Width;
                int screenHeight = primaryScreen.WorkingArea.Height;

                form.Text = caption;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.ShowInTaskbar = false;
                form.AutoSize = true; // �t�H�[���̃T�C�Y����������
                form.AutoSizeMode = AutoSizeMode.GrowAndShrink; // �t�H�[���̃T�C�Y����������

                var mainPanel = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    Padding = new Padding(padding),
                    BackColor = Color.White,
                    AutoSize = true, // �p�l���̃T�C�Y����������
                    AutoSizeMode = AutoSizeMode.GrowAndShrink // �p�l���̃T�C�Y����������
                };
                mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                form.Controls.Add(mainPanel);

                // �A�C�R����ǉ��i�I�v�V�����j
                if (icon != MessageBoxIcon.None)
                {
                    var pictureBox = new PictureBox
                    {
                        Image = GetIcon(icon),
                        SizeMode = PictureBoxSizeMode.CenterImage,
                        Dock = DockStyle.Fill,
                        Width = 48,
                        Height = 48
                    };
                    mainPanel.Controls.Add(pictureBox, 0, 0);

                    // �A�C�R���ɉ���������炷
                    PlaySoundForIcon(icon);
                }

                

                // �e�L�X�g���E�����ɂ��Ēǉ�
                var label = new Label
                {
                    Text = text,
                    AutoSize = true,
                    MaximumSize = new Size(screenWidth - 400, screenHeight - 400),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Fill
                };
                mainPanel.Controls.Add(label, 1, 0);

                // �{�^���p�l����ǉ�
                var panel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Bottom,
                    FlowDirection = FlowDirection.RightToLeft,
                    Padding = new Padding(5),
                    Height = 40,
                    AutoSize = true, // �p�l���̃T�C�Y����������
                    AutoSizeMode = AutoSizeMode.GrowAndShrink // �p�l���̃T�C�Y����������
                };
                form.Controls.Add(panel);

                DialogResult result = DialogResult.None;

                void AddButton(string buttonText, DialogResult dialogResult)
                {
                    var button = new Button
                    {
                        Text = buttonText,
                        DialogResult = dialogResult,
                        AutoSize = true
                    };
                    button.Click += (sender, e) => { result = dialogResult; form.Close(); };
                    panel.Controls.Add(button);
                }

                // �{�^����ǉ�
                switch (buttons)
                {
                    case MessageBoxButtons.OK:
                        AddButton("OK", DialogResult.OK);
                        break;
                    case MessageBoxButtons.OKCancel:
                        AddButton("�L�����Z��", DialogResult.Cancel);
                        AddButton("OK", DialogResult.OK);
                        break;
                    case MessageBoxButtons.YesNo:
                        AddButton("������", DialogResult.No);
                        AddButton("�͂�", DialogResult.Yes);
                        break;
                    case MessageBoxButtons.YesNoCancel:
                        AddButton("�X�L�b�v", DialogResult.Cancel);
                        AddButton("���O�̕ύX", DialogResult.No);
                        AddButton("�㏑��", DialogResult.Yes);
                        break;
                    default:
                        throw new NotSupportedException("Unsupported button type");
                }

                form.AcceptButton = panel.Controls[panel.Controls.Count - 1] as Button;
                form.CancelButton = panel.Controls[0] as Button;

                // Show the form and return the result
                form.ShowDialog();
                return result;
            }
        }

        // �A�C�R�����擾����w���p�[���\�b�h
        private static Image? GetIcon(MessageBoxIcon icon)
        {
            return icon switch
            {
                MessageBoxIcon.Information => SystemIcons.Information.ToBitmap(),
                MessageBoxIcon.Warning => SystemIcons.Warning.ToBitmap(),
                MessageBoxIcon.Error => SystemIcons.Error.ToBitmap(),
                MessageBoxIcon.Question => SystemIcons.Question.ToBitmap(),
                MessageBoxIcon.None => SystemIcons.Information.ToBitmap(),
                _ => null
            };
        }

        // �A�C�R���ɉ���������炷�w���p�[���\�b�h
        private static void PlaySoundForIcon(MessageBoxIcon icon)
        {
            switch (icon)
            {
                case MessageBoxIcon.Information:
                    SystemSounds.Asterisk.Play();  // ���
                    break;
                case MessageBoxIcon.Warning:
                    SystemSounds.Exclamation.Play();  // �x����
                    break;
                case MessageBoxIcon.Error:
                    SystemSounds.Hand.Play();  // �G���[��
                    break;
                case MessageBoxIcon.Question:
                    SystemSounds.Question.Play();  // ���≹
                    break;
                default:
                    break;
            }
        }
    }

    class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

        private const int SYMBOLIC_LINK_FLAG_FILE = 0x0;
        private const int SYMBOLIC_LINK_FLAG_DIRECTORY = 0x1;

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                MessageBox.Show("�������s�����Ă��܂��B�\�[�X�t�@�C��/�f�B���N�g���ƃ����N��f�B���N�g�����w�肵�Ă��������B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsUserAnAdmin())
            {
                ElevateToAdmin(args);
                return;
            }

            string targetDirectory = args[^1]; // �Ō�̈����������N���쐬����f�B���N�g��
            string[] sourcePaths = args[..^1]; // �Ō�̈����ȊO���\�[�X�p�X

            if (!Directory.Exists(targetDirectory))
            {
                MessageBox.Show("�����N��f�B���N�g�������݂��܂���B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            foreach (string sourcePath in sourcePaths)
            {
                if (!File.Exists(sourcePath) && !Directory.Exists(sourcePath))
                {
                    CustomMessageBox.Show($"�\�[�X�p�X���s���ł�: \n\"{sourcePath}\"", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }


                string sourceName = Path.GetFileName(sourcePath); // �\�[�X�̖��O���擾
                string linkPath = Path.Combine(targetDirectory, sourceName);

                // �����N�����ɑ��݂���ꍇ
                while (File.Exists(linkPath) || Directory.Exists(linkPath))
                {
                    var result = CustomMessageBox.Show(
                        $"{linkPath} \n�͊��ɑ��݂��܂��B�ǂ����܂����H",
                        "�m�F",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Exclamation
                    );

                    if (result == DialogResult.Yes) // �㏑��
                    {
                        try
                        {
                            if (File.Exists(linkPath))
                                File.Delete(linkPath); // �t�@�C�����폜
                            if (Directory.Exists(linkPath))
                                Directory.Delete(linkPath, true); // �f�B���N�g�����폜
                        }
                        catch (Exception ex)
                        {
                            CustomMessageBox.Show($"�폜�Ɏ��s���܂���: \n{ex.Message}", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            continue;
                        }
                        break; // �㏑���̏ꍇ�A���[�v�𔲂���
                    }
                    else if (result == DialogResult.No) // �ԍ���t�^
                    {
                        string baseName = Path.GetFileNameWithoutExtension(sourceName);
                        string extension = Path.GetExtension(sourceName);
                        int counter = 1;

                        do
                        {
                            linkPath = Path.Combine(targetDirectory, $"{baseName}_{counter}{extension}");
                            counter++;
                        } while (File.Exists(linkPath) || Directory.Exists(linkPath));
                    }
                    else if (result == DialogResult.Cancel) // �L�����Z��
                    {
                        linkPath = string.Empty;
                        break;
                    }
                    else
                    {
                        CustomMessageBox.Show("�s���ȃG���[���������܂����B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        linkPath = string.Empty;
                        break;
                    }
                }

                if (linkPath == string.Empty)
                {
                    continue;
                }

                // �V���{���b�N�����N�̍쐬
                bool isDirectory = Directory.Exists(sourcePath);
                bool success = CreateSymbolicLink(linkPath, sourcePath, isDirectory ? SYMBOLIC_LINK_FLAG_DIRECTORY : SYMBOLIC_LINK_FLAG_FILE);

                if (!success)
                {
                    CustomMessageBox.Show($"�V���{���b�N�����N�̍쐬�Ɏ��s���܂���: \n{sourcePath}", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            MessageBox.Show("���삪�������܂����B", "����", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static bool IsUserAnAdmin()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static void ElevateToAdmin(string[] args)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = Application.ExecutablePath,
                Arguments = string.Join(" ", Array.ConvertAll(args, arg => $"\"{arg}\"")), // ���������p���ň͂�
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                Process.Start(processInfo);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"�Ǘ��Ҍ����ł̋N���Ɏ��s���܂����B\n{ex.Message}", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
