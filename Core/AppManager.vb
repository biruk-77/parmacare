Imports System.Drawing

Namespace Core
    Public Class AppManager
        Public Shared Property IsDarkMode As Boolean = False
        Public Shared Property CurrentLang As String = "EN"

        Shared Sub New()
            DatabaseManager.InitializeDatabase()
        End Sub

        Public Structure Theme
            Public Background As Color
            Public Surface As Color
            Public Accent As Color
            Public PrimaryText As Color
            Public SecondaryText As Color
            Public InputBackground As Color
            Public InputBorder As Color
            Public DividerColor As Color
            Public SelectionBackground As Color
            Public SelectionText As Color
        End Structure

        Public Shared ReadOnly Property CurrentTheme As Theme
            Get
                If IsDarkMode Then
                    Return New Theme With {
                        .Background = Color.FromArgb(16, 34, 29),
                        .Surface = Color.FromArgb(15, 23, 42),
                        .Accent = Color.FromArgb(19, 236, 182),
                        .PrimaryText = Color.FromArgb(241, 245, 249),
                        .SecondaryText = Color.FromArgb(148, 163, 184),
                        .InputBackground = Color.FromArgb(15, 23, 42),
                        .InputBorder = Color.FromArgb(30, 41, 59),
                        .DividerColor = Color.FromArgb(30, 41, 59),
                        .SelectionBackground = Color.FromArgb(30, 64, 175), ' Deep Indigo
                        .SelectionText = Color.White
                    }
                Else
                    Return New Theme With {
                        .Background = Color.FromArgb(246, 248, 248),
                        .Surface = Color.White,
                        .Accent = Color.FromArgb(19, 236, 182),
                        .PrimaryText = Color.FromArgb(15, 23, 42),
                        .SecondaryText = Color.FromArgb(100, 116, 139),
                        .InputBackground = Color.White,
                        .InputBorder = Color.FromArgb(226, 232, 240),
                        .DividerColor = Color.FromArgb(241, 245, 249),
                        .SelectionBackground = Color.FromArgb(37, 99, 235), ' Focused Royal Blue
                        .SelectionText = Color.White
                    }
                End If
            End Get
        End Property

        Private Shared Function L(en As String, am As String, om As String) As String
            Select Case CurrentLang
                Case "AM" : Return am
                Case "OM" : Return om
                Case Else : Return en
            End Select
        End Function

        Public Shared Function GetText(key As String) As String
            Select Case key
                ' Branding
                Case "Brand" : Return "PharmaCare"
                Case "SidebarTitle" : Return L("Smart Pharmacy Management", "ዘመናዊ የመድኃኒት ቤት አስተዳደር", "Bulchiinsa Qorichaa Ammayyaa")
                Case "SidebarDesc" : Return L("Access your clinical dashboard, manage inventory, and provide exceptional patient care with PharmaCare's integrated ecosystem.", "ክሊኒካዊ ዳሽቦርድዎን ይድረሱ፣ ክምችት ያስዳድሩ እና ልዩ የታካሚ እንክብካቤ ይስጡ።", "Daaashboordii keessan banachuu, qabeenya bulchuu fi tajaajila fayyaa kennuu.")
                
                ' Right Panel Header
                Case "AdminLogin" : Return L("Staff Portal Login", "የሠራተኛ መግቢያ", "Seensa Hojjetaa")
                Case "AdminDesc" : Return L("Please enter your credentials to access the pharmacy system.", "እባክዎ ፋርማሲ ሲስተም ለመግባት የመለያዎን ማረጋገጫ ያስገቡ።", "Maaloo odeeffannoo keessan galchuun seenaa.")

                ' Inputs
                Case "UserLabel" : Return L("Username or Email", "የተጠቃሚ ስም ወይም ኢሜይል", "Maqaa Fayyadamaa")
                Case "UserPlaceholder" : Return L("admin@pharmacare.com", "admin@pharmacare.com", "admin@pharmacare.com")
                Case "PassLabel" : Return L("Password", "የይለፍ ቃል", "Jecha Icchitii")
                Case "ForgotPass" : Return L("Forgot password?", "የይለፍ ቃል ረሱ?", "Jecha dagattee?")
                Case "RememberMe" : Return L("Remember this terminal", "ይህን ተርምናል አስታውስ", "Terminal kana yaadadhu")

                ' Buttons
                Case "LoginBtn" : Return L("Sign In to System", "ወደ ሲስተም ግቡ", "Gara Sirnichaatti Seeni")
                Case "Authenticating" : Return L("SIGNING IN...", "በማረጋገጥ ላይ...", "SEENAA JIRA...")

                ' Footer
                Case "NewStaff" : Return L("New staff member?", "አዲስ ሠራተኛ?", "Hojjetaa haaraa?")
                Case "RequestAccount" : Return L("Request account", "መለያ ይጠይቁ", "Herrega gaafadhu")
                Case "AlreadyStaff" : Return L("Already have an account?", "መለያ አለዎት?", "Herrega qabduu?")
                Case "RegisterTitle" : Return L("Staff Registration", "የሠራተኛ ምዝገባ", "Galmee Hojjetaa")
                Case "RegisterDesc" : Return L("Create your credentials to access the pharmacy ecosystem.", "የፋርማሲውን ሲስተም ለመጠቀም መለያዎን ይፍጠሩ።", "Nizama qorichaa fayyadamuuf herrega keessan uumaa.")
                Case "FullNameLabel" : Return L("Full Name", "ሙሉ ስም", "Maqaa Guutuu")
                Case "RoleLabel" : Return L("Select Your Role", "ሥራዎን ይምረጡ", "Gahee keessan filadhaa")
                Case "RegisterBtn" : Return L("Complete Registration", "ምዝገባውን ያጠናቅቁ", "Galmee Xumuri")
                Case "RegSuccess" : Return L("Registration Successful! Please login.", "ምዝገባው ተሳክቷል! እባክዎ ይግቡ።", "Galmeen milkaa'eera! Maaloo seeni.")
                Case "AdminRole" : Return L("Admin", "አስተዳዳሪ", "Bulchaa")
                Case "PharmacistRole" : Return L("Pharmacist", "ፋርማሲስት", "Faarmasisti")
                Case "ClerkRole" : Return L("Inventory Clerk", "ክምችት ተቆጣጣሪ", "Klerika")
                Case "ContactAdmin" : Return L("Contact Admin", "አስተዳዳሪውን ያግኙ", "Bulchaa Quunnamaa")
                Case "HelpCenter" : Return L("Help Center", "የእገዛ ማዕከል", "Giddugala Gargaarsaa")
                Case "Copyright" : Return L("© 2024 PharmaCare Systems Inc. All Rights Reserved.", "© 2024 ፋርማኬር ሲስተምስ ኢንክ. መብቶች የተጠበቁ ናቸው።", "© 2024 PharmaCare Systems Inc. Mirgi Seeraan Eegama.")

                ' Forgot Password Screen
                Case "ForgotTitle" : Return L("Reset Password", "የይለፍ ቃል ይቀይሩ", "Jecha Icchitii Jijjiiri")
                Case "ForgotDesc" : Return L("Enter your @username and a new password to reset it.", "የመልሶ ማግኛ አገናኝ ለማግኘት ኢሜልዎን ያስገቡ።", "Ergaa deebisii argachuuf email keessan galchaa.")
                Case "SendReset" : Return L("Reset Password", "አገናኝ ላክ", "Ergaa Ergi")
                
                ' About Us Screen
                Case "AboutTitle" : Return L("About PharmaCare", "ስለ ፋርማኬር", "Waa'ee PharmaCare")
                Case "AboutDesc" : Return L("PharmaCare is a leading provider of smart pharmacy management solutions, dedicated to excellence in patient care and operational efficiency.", "ፋርማኬር በታካሚ እንክብካቤ እና በሥራ ቅልጥፍ ንቁ የሆነ ግንባር ቀደም ዘመናዊ የመድኃኒት ቤት አስተዳደር መፍትሔ አቅራቢ ነው።", "PharmaCare dhaabbata bulchiinsa qorichaa ammayyaa ta'ee, tajaajila qulqulluu kennuuf kan hojjetudha.")
                Case "ContactInfo" : Return L("Emergency Support: +251 911 000 000", "የአደጋ ጊዜ ድጋፍ: +251 911 000 000", "Tajaajila Ariifachiisaa: +251 911 000 000")
                
                ' General Navigation
                Case "BackToLogin" : Return L("Back to Login", "ወደ መግቢያ ተመለስ", "Gara Seensaa Deebi'i")
                Case "Back" : Return L("Back", "ተመለስ", "Deebi'i")
                
                ' Dashboard Sidebar Menu
                Case "MenuOVERVIEW" : Return L("DASHBOARD", "ዳሽቦርድ", "DAASHBOORDII")
                Case "MenuINVENTORY" : Return L("INVENTORY", "ክምችት", "QABEENYA")
                Case "MenuPOS" : Return L("POS / SALES", "ሽያጭ", "GURGURTAA")
                Case "MenuSUPPLIERS" : Return L("SUPPLIERS", "አቅራቢዎች", "DHIYEESSITOOTA")
                Case "MenuREPORTS" : Return L("REPORTS", "ሪፖርቶች", "GABAAASA")
                Case "MenuUSERS" : Return L("MANAGE USERS", "ሠራተኞች", "FAYYADAMTOOTA")
                Case "MenuSETTINGS" : Return L("SETTINGS", "ቅንብሮች", "QINDIYEE")
                Case "MenuLOGOUT" : Return L("LOGOUT", "ውጣ", "Bahuu")

                ' Dashboard
                Case "DashOverview" : Return L("Dashboard Overview", "የዳሽቦርድ አጠቃላይ እይታ", "Ilaalcha Waliigalaa Daashboordii")
                Case "WelcomeAdmin" : Return L("Welcome back, Administrator. Here's your pharmacy status.", "እንኳን ደህና መጡ፣ አስተዳዳሪ። የፋርማሲዎ ሁኔታ ይኸውና።", "Baga nagaan deebite, Bulchaa. Haalli faarmasii keessani kanadha.")
                Case "TotalProducts" : Return L("Total Products", "ጠቅላላ ምርቶች", "Waliigala Oomishootaa")
                Case "LowStock" : Return L("Low Stock Items", "ያለቁ ምርቶች", "Oomishoota dhumachaa jiran")
                Case "ExpiringSoon" : Return L("Expiring Soon", "ጊዜያቸው የሚያልፍባቸው", "Kan yeroon irra darbu")
                Case "SalesActivity" : Return L("Sales Activity (Last 7 Days)", "የሽያጭ እንቅስቃሴ (ያለፉት 7 ቀናት)", "Sochiilee Gurgurtaa (Guyyoota 7n darban)")

                ' Modules
                Case "ManageUsersDesc" : Return L("Oversee all pharmacy staff and administrative accounts.", "ሁሉንም የፋርማሲ ሠራተኞች እና አስተዳደራዊ መለያዎችን ያቀናብሩ።", "Hojjettoota faarmasii fi herreega bulchiinsaa hunda to'adhu.")
                Case "AddUserBtn" : Return L("Add New User", "አዲስ ሠራተኛ ያክሉ", "Fayyadamaa Haaraa Ida'i")
                Case "InventoryDesc" : Return L("Global inventory tracking and batch management.", "ዓለም አቀፍ የእቃ ክምችት እና የባች አስተዳደር።", "Hordoffii qabeenyaa waliigalaa fi bulchiinsa baachii.")
                Case "AddProductBtn" : Return L("Add New Product", "አዲስ ምርት ያክሉ", "Oomisha Haaraa Ida'i")
                Case "POSDesc" : Return L("Process sales and manage retail transactions.", "ሽያጮችን ያካሂዱ እና የችርቻሮ ግብይቶችን ያስተዳድሩ።", "Gurgurtaa raawwadhu fi daldala daldalaa bulchi.")
                
                ' Added for new MDI Container
                Case "MenuFILE" : Return L("FILE", "ፋይል", "FAAYILII")
                Case "MenuEXIT" : Return L("Exit Program", "ከპሮግራም ውጣ", "Sirnicha Keessaa Bahi")
                Case "MenuMODULES" : Return L("MODULES", "ሞጁሎች", "KUTAALEE")
                Case "MenuView" : Return L("VIEW", "እይታ", "ILAALCHI")
                Case "MenuToggleTheme" : Return L("Toggle Dark/Light Theme", "ገጽታ ቀይር (ጨለማ/ብርሃን)", "Faaya Jijjiiri (Dukkana/Ifa)")
                Case "MenuToggleLanguage" : Return L("Switch Language", "ቋንቋ ቀይር", "Afaan Jijjiiri")
                Case "MenuWINDOW" : Return L("WINDOW", "መስኮት", "FODDAA")
                Case "SystemReady" : Return L("System Ready.", "ሲስተሙ ዝግጁ ነው።", "Sirnichi Qophaayera.")

                Case Else : Return key
            End Select
        End Function

        Public Shared Sub ApplyGridTheme(dgv As DataGridView)
            Dim theme = CurrentTheme
            dgv.BackgroundColor = theme.Surface
            dgv.GridColor = theme.DividerColor
            dgv.BorderStyle = BorderStyle.None
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None
            dgv.EnableHeadersVisualStyles = False
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            dgv.RowHeadersVisible = False
            dgv.AllowUserToResizeRows = False
            dgv.RowTemplate.Height = 45

            ' Header Style
            dgv.ColumnHeadersHeight = 40
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            dgv.ColumnHeadersDefaultCellStyle = New DataGridViewCellStyle With {
                .BackColor = theme.Background,
                .ForeColor = theme.SecondaryText,
                .Font = New Font("Segoe UI Semibold", 10),
                .Padding = New Padding(10, 10, 10, 10),
                .SelectionBackColor = theme.Background
            }

            ' Default Cell Style
            dgv.DefaultCellStyle = New DataGridViewCellStyle With {
                .BackColor = theme.Surface,
                .ForeColor = theme.PrimaryText,
                .Font = New Font("Segoe UI", 10),
                .SelectionBackColor = theme.SelectionBackground,
                .SelectionForeColor = theme.SelectionText,
                .Padding = New Padding(10, 0, 10, 0)
            }

            dgv.AlternatingRowsDefaultCellStyle = New DataGridViewCellStyle With {
                .BackColor = If(IsDarkMode, Color.FromArgb(22, 30, 45), Color.FromArgb(248, 250, 252))
            }
        End Sub
    End Class
End Namespace
