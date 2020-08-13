Imports System.Windows.Forms

Class Debuger
	''' <summary>
	''' Příkazy, které se budou debugovat
	''' </summary>
	Private prikazy() As String

	''' <summary>
	''' Obsluha kliku tlačítka btnEnd
	''' </summary>
	''' <param name="sender">odesílatel (tlačítko)</param>
	''' <param name="e">argumenty</param>
	Private Sub btnEnd_Click(sender As Object, e As EventArgs) Handles btnEnd.Click
		RaiseEvent Udalost(Me, New UdalostEventHandler(UdalostEventHandler.Typy.konec))
	End Sub

	''' <summary>
	''' Konstruktor, načte příkazy a připraví okno
	''' </summary>
	''' <param name="prikazy">Příkazy</param>
	Public Sub New(prikazy() As String)
		InitializeComponent()
		NactiPrikazy(prikazy)
	End Sub

	''' <summary>
	''' Načte příkazy
	''' </summary>
	''' <param name="prikazy">Příkazy</param>
	Private Sub NactiPrikazy(prikazy() As String)
		' Uložíme si příkazy
		Me.prikazy = prikazy
		' Projdeme je
		For Each prikaz In prikazy
			' Naplníme list
			listPrikazy.Items.Add(prikaz)
		Next
		' Vybereme první
		listPrikazy.SelectedIndex = 0
	End Sub

	''' <summary>
	''' Napíše zprávu do konzole
	''' </summary>
	''' <param name="text">Zpráva, která se vypíše</param>
	''' <param name="typ">Typ vypisované zprávy</param>
	Public Sub NapisDoKonzole(text As String,
							  Optional typ As typyZprav = typyZprav.informace)
		' Doplníme předponu (Varování|CHYBA)
		Dim predponaZpravy As String
		If typ = typyZprav.chyba Then
			predponaZpravy = "CHYBA: "
		ElseIf typ = typyZprav.informace Then
			predponaZpravy = ""
		Else
			predponaZpravy = "Varování: "
		End If
		' přídame do konzole
		txtKonzole.Text &= predponaZpravy & text & vbCrLf
	End Sub

	''' <summary>
	''' Zavřeme okno debugeru
	''' </summary>
	Public Sub Konec()
		Close()
	End Sub

	''' <summary>
	''' Přičte položku, pokud dojde k chybě, předáme to interpretu přes událost
	''' </summary>
	Public Sub PrictiPolozku()
		Try
			' skusíme přičíst
			listPrikazy.SelectedIndex += 1
		Catch ex As Exception
			' předáme zprávu interpretu
			RaiseEvent Udalost(Me, New UdalostEventHandler(
							   UdalostEventHandler.Typy.konec))
		End Try
	End Sub

	''' <summary>
	''' Krok, obsloužíme krok buď časovače nebo uživatele
	''' </summary>
	''' <param name="sender">Odesílatel</param>
	''' <param name="e">Argumenty</param>
	Public Sub Krok(sender As Object, e As EventArgs) Handles btnStep.Click
		' Pokud je odesílatel tlačítko
		If TypeOf sender Is Button Then
			RaiseEvent Udalost(Me, New UdalostEventHandler(
							   UdalostEventHandler.Typy.uzivateluvKrok))
		Else ' ne, odesílatel je časovač
			RaiseEvent Udalost(Me, New UdalostEventHandler(
							   UdalostEventHandler.Typy.casovacuvKrok))
		End If
	End Sub

	''' <summary>
	''' Událost debugeru
	''' </summary>
	Public Event Udalost As EventHandler
End Class