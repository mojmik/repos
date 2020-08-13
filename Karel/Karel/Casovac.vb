Class Casovac
	''' <summary>
	''' Nastaví interval časovači
	''' </summary>
	''' <returns>Hodnotu intervalu (v sekundách)</returns>
	Public Property interval As Integer
		Get
			Return tmr.Interval / 1000
		End Get
		Set(value As Integer)
			tmr.Interval = value * 1000
		End Set
	End Property

	''' <summary>
	''' Timer - srdce časovače
	''' </summary>
	Private WithEvents tmr As Timer = New Timer()

	''' <summary>
	''' Zapne časovač
	''' </summary>
	Public Sub Zapni()
		tmr.Enabled = True
	End Sub

	''' <summary>
	''' Vypne časovač
	''' </summary>
	Public Sub Vypni()
		tmr.Enabled = False
	End Sub

	''' <summary>
	''' Vytvoří nový časovač a nastaví interval
	''' </summary>
	''' <param name="interval">Nový interval</param>
	Public Sub New(interval As Integer)
		Me.tmr.Interval = interval
	End Sub

	''' <summary>
	''' Vyvolá událost Krok
	''' </summary>
	Private Sub tmr_Tick() Handles tmr.Tick
		RaiseEvent Krok(Me, New EventArgs())
	End Sub

	''' <summary>
	''' Událost Krok, vyvolá ji časovač.
	''' </summary>
	''' <remarks></remarks>
	Public Event Krok As EventHandler
End Class
