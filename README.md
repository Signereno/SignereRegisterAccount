# Signere.no registrere kundekonto

####Klient progam for å etablere kundeforhold til Signere.no

Dette gjør det veldig lett og raskt å implementere Signere.no i dine løsninger og etablere kontoer for dine kunder.

Programmet er en windows exe fil som krever .NET 3.5 installert. Den åpner en nettside med skjema som bruker fyller ut og signerer. Det blir da opprettet en konto hos Signere.no, og kontoinformasjon blir lastet ned til angitt filsti. For deretter å kunne lagres i systemet en ønsker å bruke Signere.no tjenester fra.

#####Dette gjør det veldig enkelt å integrere Signere.no sine tjenester i egen programvare. Signere.no enkelt å komme i gang og enkelt i bruk.

Last ned exe fil her: <a href="https://github.com/Signereno/SignereRegisterAccount/blob/master/dist/SignereRegisterAccount.exe" title="Last ned exe fil">Last ned</a>

###Prosessen
1. Start exe fil med parametere fra eget program
2. Programmet laster skjema tilpasset hver forhandler (logo, stilark, informasjon o.l.)
3. Kunde fyller ut skjema, Organisasjonsnavn, Orgnummer, kontaktperson osv.
4. Kunde signerer avtale med BankID. Hver avtale dokument kan tilpasses enkeltvis for forhandlere etter behov.
5. Etter gjennomført signatur opprettes det en demo konto hos Signere.no tilknyttet din forhandlerid (se nedenfor)
6. Sendes ut kvitteringse-post til kunde, forhandler og til Signere.no sin salgsavdeling
7. Konto informasjon lagres som en fil på maskinen til personen som signerte og kan legges inn i programmet og dermed er en klar til å bruke Signere.no i sitt program.
8. Dersom kunde ønsker å bli kunde, tar en kontakt med forhandler eller Signere.no og konto omgjøres fra demo konto til vanlig konto.
9. Kunden er nå klar for Norges enkleste Signerings tjeneste :-)

###Input parameters:
1. DealerID (Guid) din forhandler ID (må være Signere.no forhandler for å få dette ta kontakt med salg@signere.no for å bli forhandler)
2. Register URL denne kan du få fra Signere.no salg, dette er en tilpasset web side med skjema for bestilling av konto. Kan flette inn informasjon til skjema via parametre i URL.
3. Filsti til lisesnfil eller json/xml fil (se pkt. 4 formater)
4. Format lic (lisensfil) Json (Tilgangsnøkler og kontoid i json format) Xml (samme som json bare i Xml format) (optional standard er lic)

#### Register URL
Register URL fra punkt 2 ovenfor kan ta inn følgende paramtere i URL strengen:
* DealerId: Guid forhandler ID
* mvanumber: Org nummer for organisasjonen som skal knyttes til kontoen
* dealerRef: Organisasjonens kundenummer eller lignende hos forhandler, brukes ved fakturering fra Signere.no til forhandler
* contactFirstName: Kontaktperson for organisasjonen fornavn
* contactLastName: Kontaktperson for organisasjonen etternavn
* contactEmail: Kontaktperson for organisasjonen e-postadresse
* contactPhone: Kontaktperson for organisasjonen telefonnummer
* contactMobile: Kontaktperson for organisasjonen mobilnummer
 
Ved å fylle ut disse fra forhandler sitt system, slipper bruker å fylle ut og registeringsprosessen blir enklere for sluttbrukeren.



### Feil koder:
<ol start="0">
 <li>Alt gikk bra</li>
 <li>Bruker avbrøt prossessen</li>
 <li>Feil med input paramtere</li>
 <li>Generell feil
     31 Filsti i parameter 3 finnes allerede</br>
  32 Feil med å hente ned lisensinformasjon fra API</br>
  33 Feil som er oppstått i registeringen eller signeringen på web</br>
  34 Filsti i parameter 3 er ugyldig</br></li>
  <li>Feil antall parametre gyldig antall er 3 eller 4 (format er optional)
 </ol>

