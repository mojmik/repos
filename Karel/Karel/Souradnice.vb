Structure Souradnice
	Public x As Integer
	Public y As Integer

	''' <summary>
	''' Nastaví nové souřadnice
	''' </summary>
	Public Sub New(x As Integer, y As Integer)
		Me.x = x
		Me.y = y
	End Sub

	''' <summary>
	''' Operator pro sčítání souřadnic
	''' </summary>
	''' <returns>Nové souřadnice</returns>
	Shared Operator +(prvni As Souradnice, druhy As Souradnice)
		Return New Souradnice(prvni.x + druhy.x, prvni.y + druhy.y)
	End Operator

	''' <summary>
	''' Vrátí souřadnice 
	''' </summary>
	''' <returns>Souřadnice ve stringu</returns>
	Public Overrides Function ToString() As String
		Return String.Format("X: {0} Y: {1}", x, y)
	End Function
End Structure