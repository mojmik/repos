Class Interpret
	''' <summary>
	''' Kreslic nám bude vykreslovat interpretované změny
	''' </summary>
	Private kreslic As Kreslic

	''' <summary>
	''' Příkazy, které budeme interpretovat
	''' </summary>
	Private prikazy() As String

	
	Private _prikaz As Integer = 0
	''' <summary>
	''' index právě interpretovaného příkazu
	''' </summary>
	Private Property prikaz As Integer
		Get
			Return _prikaz
		End Get
		Set(value As Integer)
			If value <> prikazy.Length Then
				_prikaz = value
			Else
				Throw New IndexOutOfRangeException
			End If
		End Set
	End Property

	''' <summary>
	''' Okno debugeru
	''' </summary>
	Private WithEvents debuger As Debuger
	''' <summary>
	''' Informace, zdali si uživatel přeje Karla krokovat
	''' </summary>
	''' <remarks></remarks>
	Private debug As Boolean

	' Jednotlivé příkazy
	Private Sub KROK()
		Dim souradniceK As Souradnice
		Select Case Karel.smer
			Case Global.Karel.Karel.Smery.doleva
				souradniceK = New Souradnice(-1, 0)
			Case Global.Karel.Karel.Smery.dolu
				souradniceK = New Souradnice(0, 1)
			Case Global.Karel.Karel.Smery.doprava
				souradniceK = New Souradnice(1, 0)
			Case Global.Karel.Karel.Smery.nahoru
				souradniceK = New Souradnice(0, -1)
		End Select
		Dim noveSouradnice As Souradnice = Karel.pozice + souradniceK
		If Karel.mapa(noveSouradnice.x, noveSouradnice.y).TypPolicka =
			Policko.TypyPolicka.zed Then
			debuger.NapisDoKonzole("Karel narazil do zdi", typyZprav.chyba)
			Return
		End If
		Karel.pozice = Karel.pozice + souradniceK
	End Sub
	Private Sub VLEVO_VBOK()
		If CInt(Karel.smer) = 0 Then
			Karel.smer = 3
		Else
			Karel.smer -= 1
		End If
	End Sub
	Private Sub POLOZ()
		Karel.mapa(Karel.pozice.x, Karel.pozice.y).pocetZnacek += 1
	End Sub
	Private Sub VEZMI()
		Karel.mapa(Karel.pozice.x, Karel.pozice.y).pocetZnacek -= 1
	End Sub
	Private Sub OTOCDOPRAVA()
		Karel.smer = Global.Karel.Karel.Smery.doprava
	End Sub
	Private Sub OTOCDOLEVA()
		Karel.smer = Global.Karel.Karel.Smery.doleva
	End Sub
	Private Sub OTOCNAHORU()
		Karel.smer = Global.Karel.Karel.Smery.nahoru
	End Sub
	Private Sub OTOCDOLU()
		Karel.smer = Global.Karel.Karel.Smery.dolu
	End Sub

	''' <summary>
	''' Interpretuje příkaz
	''' </summary>
	''' <param name="prikaz">příkaz k interpretaci</param>
	Private Sub ZpracujPrikaz(prikaz As String)
		' Příkaz může být nothing
		Try
			prikaz = prikaz.Trim()
			prikaz = prikaz.ToUpper()
			Select Case prikaz
				Case "KROK"
					KROK()
				Case "VLEVO-VBOK"
					VLEVO_VBOK()
				Case "POLOZ"
					POLOZ()
				Case "VEZMI"
					VEZMI()
				Case "OTOC-DOPRAVA"
					OTOCDOPRAVA()
				Case "OTOC-DOLEVA"
					OTOCDOLEVA()
				Case "OTOC-NAHORU"
					OTOCNAHORU()
				Case "OTOC-DOLU"
					OTOCDOLU()
				Case Else
					debuger.NapisDoKonzole(String.Format("Příkaz {0} není platný",
														 prikaz), typyZprav.chyba)
			End Select
			kreslic.VykresliPolickaKolemKarla()
			debuger.PrictiPolozku()
		Catch ex As Exception
			' Příkazy došly, vyolej konec
			UdalostDebugeru(Me, New UdalostEventHandler(UdalostEventHandler.Typy.konec))
		End Try
	End Sub
	Public Sub New(kreslic As Kreslic, prikazy() As String, debug As Boolean)
		Me.kreslic = kreslic
		Me.prikazy = prikazy
		debuger = New Debuger(prikazy)
		Me.debug = debug
		debuger.btnStep.Enabled = debug
		debuger.Show()
	End Sub
	Public Sub ZpraucjDalsiprikaz()
		Try
			ZpracujPrikaz(prikazy(prikaz))
		Catch ex As IndexOutOfRangeException
			UdalostDebugeru(Me, New UdalostEventHandler(UdalostEventHandler.Typy.konec))
		End Try
		Try
			prikaz += 1
		Catch ex As IndexOutOfRangeException
			UdalostDebugeru(Me, New UdalostEventHandler(UdalostEventHandler.Typy.konec))
		End Try
	End Sub

	Public Sub UdalostDebugeru(sender As Object, e As UdalostEventHandler) Handles debuger.Udalost
		If e.typ = UdalostEventHandler.Typy.konec Then
			debuger.Konec()
			Array.Clear(prikazy, 0, prikazy.Length)
		ElseIf e.typ = UdalostEventHandler.Typy.uzivateluvKrok Then
			ZpraucjDalsiprikaz()
		End If
	End Sub
End Class
