Module Karel
	Public mapa(9, 9) As Policko

	Private _pozice As New Souradnice(5, 5)
	''' <summary>
	''' Pozice Karla, při změně Karla posune
	''' </summary>
	''' <value>Pozice Karla</value>
	''' <returns>Pozice Karla</returns>
	Public Property pozice As Souradnice
		Get
			Return _pozice
		End Get
		Set(value As Souradnice)
			mapa(_pozice.x, _pozice.y).TypPolicka = Policko.TypyPolicka.nic
			_pozice = value
			mapa(_pozice.x, _pozice.y).TypPolicka = Policko.TypyPolicka.Karel
		End Set
	End Property

	''' <summary>
	''' Obrázek Karla, mění se se směrem
	''' </summary>
	Public KarluvObrazek As Image

	Private _smer As Smery
	''' <summary>
	''' Směr pohybu Karla
	''' </summary>
	''' <value>Enumerace smery</value>
	''' <returns>Směr kam Karel půjde</returns>
	Public Property smer As Smery
		Get
			Return _smer
		End Get
		Set(value As Smery)
			' nastavíme směr
			_smer = value
			' nastavíme obrázek Karla, číslo získáme přetypováním smeru pohybu
			' na integer. Obrázek vezmeme v resources
			KarluvObrazek = My.Resources.ResourceManager.GetObject("Karlik" &
																   CInt(value))
		End Set
	End Property

	''' <summary>
	''' Připraví mapu
	''' </summary>
	Public Sub PripravMapu()
		' Deklarujeme všechno na 'volno'
		For x = 0 To 9
			For y = 0 To 9
				mapa(x, y) = New Policko(Policko.TypyPolicka.nic)
			Next
		Next
		' Karel je na 5 × 5
		mapa(5, 5) = New Policko(Policko.TypyPolicka.Karel)
		' půjdeme doprava
		smer = Smery.doprava
	End Sub

	''' <summary>
	''' Směry
	''' </summary>
	Public Enum Smery
		' Čísla zde jsou veledůležitá, podle nich se bere i obrázek Karla.
		doleva = 0
		nahoru = 1
		doprava = 2
		dolu = 3
	End Enum
End Module
