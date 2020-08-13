Public Class UdalostEventHandler
	Inherits EventArgs
	''' <summary>
	''' Typ události, ze srandy králikům tu není.
	''' </summary>
	Public typ As Typy
	Public Sub New(typ As Typy)
		Me.typ = typ
	End Sub
	''' <summary>
	''' Mžné typy události
	''' </summary>
	Public Enum Typy
		konec
		uzivateluvKrok
		casovacuvKrok
	End Enum
End Class
