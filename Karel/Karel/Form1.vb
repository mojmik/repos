Public Class Form1
	''' <summary>
	''' Časovač
	''' </summary>
	Private WithEvents casovac As New Casovac(2000)

	''' <summary>
	''' Kreslič
	''' </summary>
	Private kreslic As Kreslic

	''' <summary>
	''' Interpret
	''' </summary>
	Private interpret As Interpret

	''' <summary>
	''' Konstruktor, načte komponenty formuláře
	''' </summary>
	Public Sub New()
		InitializeComponent()
	End Sub

	''' <summary>
	''' Obsluha kliku tlačítka
	''' </summary>
	Private Sub Button1_Click() Handles Button1.Click
		' Vypneme časovač pokud běží
		casovac.Vypni()
		' Nový parser
		Dim parser As New Parser()
		' Parsujeme prikazy
		Dim prikazy() As String = parser.ParsujPrikazy(TextBox1.Text)
		' Nový interpret s novými příkazy
		interpret = New Interpret(kreslic, prikazy, cchDebug.Checked)
		' Pokud jedeme bez dbeugu zapneme časovač
		If Not cchDebug.Checked Then
			casovac.Zapni()
		End If
	End Sub

	''' <summary>
	''' Obsluha kroku
	''' </summary>
	Private Sub Krok() Handles casovac.Krok
		Try
			' Skusíme zpracovat příkaz
			interpret.ZpraucjDalsiprikaz()
		Catch ex As IndexOutOfRangeException
			' Došly příkazy, vypmneme časovač a debuger
			casovac.Vypni()
			interpret.UdalostDebugeru(Me, New UdalostEventHandler(
									  UdalostEventHandler.Typy.konec))
		End Try
	End Sub

	''' <summary>
	''' Změníme pozici Karla
	''' </summary>
	Private Sub ZmenaPozice() Handles nudX.ValueChanged, nudY.ValueChanged
		Try
			' Nové souřadnice
			Dim noveSouradnice As Souradnice = New Souradnice(nudX.Value, nudY.Value)
			' Karlovi nastavíme nové souřadnice
			Karel.pozice = noveSouradnice
			' vykreslíme ho a políčka kolem něj
			kreslic.VykresliPolickaKolemKarla()
		Catch ex As Exception
			' Před načtením formuláře to zde dělá bordel
		End Try
	End Sub

	''' <summary>
	''' Obsluha změny intervalu
	''' </summary>
	Private Sub nudInterval_ValueChanged() Handles nudInterval.ValueChanged
		casovac.interval = nudInterval.Value
	End Sub

	''' <summary>
	''' Obsluha načtení formuláře
	''' </summary>
	Private Sub Form1_Load() Handles Me.Load
		' Načteme krelič
		kreslic = New Kreslic(PictureBox1.CreateGraphics())
		' Připravíme mapu
		PripravMapu()
	End Sub

	''' <summary>
	''' Přidáme/odeerem zeď na políčku
	''' </summary>
	''' <param name="sender">Odesílatel</param>
	''' <param name="e">Argumenty</param>
	Private Sub PictureBox1_MouseDown(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseDown
		' Spočítáme políčko (X;Y)
		Dim x As Integer = Math.Floor(e.X / 20)
		Dim y As Integer = Math.Floor(e.Y / 20)

		' Změníme stav
		Karel.mapa(x, y).zedNeboNic()
		' Překreslíme
		kreslic.vykresliPolicko(New Souradnice(x, y))
	End Sub

	''' <summary>
	''' Obsluha překreslení mapy
	''' </summary>
	Private Sub PictureBox1_Paint() Handles PictureBox1.Paint
		For x = 0 To 9
			For y = 0 To 9
				kreslic.VykresliPolickaKolemKarla()
			Next
		Next
	End Sub

	Private Sub btnNapoveda_Click() Handles btnNapoveda.Click
		Dim napoveda As New Napoveda()
		napoveda.ShowDialog()
	End Sub
End Class
