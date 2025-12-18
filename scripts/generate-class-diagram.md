# Generowanie Diagramów Klas - Przewodnik

## Opcja 1: Visual Studio Class Designer

### Instalacja
1. Otwórz Visual Studio Installer
2. Wybierz "Modify" dla swojej instalacji Visual Studio
3. W zakładce "Individual components" zaznacz "Class Designer"
4. Kliknij "Modify" i poczekaj na instalację

### Użycie
1. Otwórz solution w Visual Studio
2. Prawym klawiszem na projekt Domain → Add → New Item
3. Wybierz "Class Diagram" (.cd)
4. Przeciągnij klasy z Solution Explorer na diagram
5. Visual Studio automatycznie wygeneruje diagram

**Zalety:**
- Wizualna edycja
- Bezpośrednia integracja z kodem
- Dwukierunkowa synchronizacja (zmiana w diagramie = zmiana w kodzie)

**Wady:**
- Tylko dla Visual Studio (nie działa w VS Code)
- Format .cd nie jest portable

## Opcja 2: Mermaid (Markdown)

### Plik już utworzony
- **Lokalizacja**: `docs/domain-model.md`
- **Format**: Markdown z blokami Mermaid

### Renderowanie
**GitHub/GitLab:**
- Automatycznie renderuje diagramy Mermaid w plikach .md

**Visual Studio Code:**
```bash
# Zainstaluj rozszerzenie
code --install-extension bierner.markdown-mermaid
```

**Generowanie PNG/SVG:**
```bash
# Zainstaluj Mermaid CLI
npm install -g @mermaid-js/mermaid-cli

# Wygeneruj obrazek
mmdc -i docs/domain-model.md -o docs/domain-model.png
```

**Zalety:**
- Czytelny tekst (łatwe review w PR)
- Automatyczne renderowanie na GitHub
- Portable i version-controllable

**Wady:**
- Trzeba ręcznie aktualizować przy zmianach kodu

## Opcja 3: PlantUML

### Plik już utworzony
- **Lokalizacja**: `docs/class-diagram.puml`

### Renderowanie

**Visual Studio Code:**
```bash
# Zainstaluj rozszerzenie
code --install-extension jebbs.plantuml

# Wymaga Java JRE
# Po zainstalowaniu: Alt+D lub Ctrl+Shift+P → "PlantUML: Preview Current Diagram"
```

**Generowanie PNG/SVG:**
```bash
# Zainstaluj PlantUML
# Windows (Chocolatey):
choco install plantuml

# Lub pobierz JAR: https://plantuml.com/download

# Wygeneruj obrazek
java -jar plantuml.jar docs/class-diagram.puml

# Wygeneruj SVG
java -jar plantuml.jar -tsvg docs/class-diagram.puml
```

**Zalety:**
- Bardzo rozbudowana składnia
- Professional-looking diagramy
- Eksport do wielu formatów (PNG, SVG, PDF)

**Wady:**
- Wymaga Java
- Bardziej złożona składnia niż Mermaid

## Opcja 4: Narzędzia automatyczne (reverse engineering)

### A. NDepend (komercyjne)
```
https://www.ndepend.com/
```
- Automatycznie generuje diagramy z kodu
- Analiza zależności i metryki
- 14-day trial

### B. Doxygen + Graphviz
```bash
# Zainstaluj Doxygen
choco install doxygen.install

# Zainstaluj Graphviz
choco install graphviz

# Utwórz plik konfiguracyjny
doxygen -g Doxyfile

# Edytuj Doxyfile:
# INPUT = src/Domain
# RECURSIVE = YES
# EXTRACT_ALL = YES
# HAVE_DOT = YES
# CLASS_DIAGRAMS = YES
# UML_LOOK = YES

# Wygeneruj dokumentację
doxygen Doxyfile
```

### C. dotnet-diagram (community tool)
```bash
# Zainstaluj global tool
dotnet tool install --global dotnet-diagram

# Wygeneruj diagram dla projektu
dotnet-diagram generate -p src/Domain/Domain.csproj -o docs/domain-diagram.png
```

## Opcja 5: draw.io / Lucidchart (ręcznie)

### draw.io (darmowe)
1. Otwórz https://app.diagrams.net/
2. Wybierz "UML" → "Class Diagram"
3. Narysuj diagram ręcznie
4. Zapisz jako .drawio.xml w repozytorium

**Integracja z VS Code:**
```bash
code --install-extension hediet.vscode-drawio
```

**Zalety:**
- Pełna kontrola wizualna
- Łatwe w użyciu
- Integracja z VS Code

**Wady:**
- Ręczna aktualizacja
- Nie generuje z kodu

## Rekomendacja dla SharpBuy

### Dla dokumentacji projektu (główne diagramy):
✅ **PlantUML** (`docs/class-diagram.puml`)
- Professional quality
- Eksport do SVG dla README.md
- Version controlled

### Dla README.md (quick overview):
✅ **Mermaid** (`docs/domain-model.md`)
- Renderuje się automatycznie na GitHub
- Czytelny w code review

### Dla eksploracji kodu (ad-hoc):
✅ **Visual Studio Class Designer**
- Szybkie tworzenie dla konkretnych klas
- Nie commituj plików .cd do repo

## Przykład użycia

### 1. Zobacz diagram w GitHub
Otwórz `docs/domain-model.md` na GitHub - diagram wyrenderuje się automatycznie

### 2. Wygeneruj PNG z PlantUML
```bash
# Zainstaluj PlantUML
java -jar plantuml.jar docs/class-diagram.puml

# Utworzy plik: docs/class-diagram.png
```

### 3. Dodaj do README
```markdown
## Domain Model

![Domain Model](docs/class-diagram.png)

See [detailed documentation](docs/domain-model.md) for more information.
```

## Utrzymanie diagramów

### Best Practices
1. **Aktualizuj po każdej zmianie domeny**
   - Dodanie nowego agregatu → aktualizuj diagram
   - Zmiana relacji → aktualizuj diagram

2. **Code review**
   - Sprawdzaj, czy PR z zmianami domeny zawiera aktualizację diagramu

3. **Automatyzacja (TODO)**
   - CI/CD może sprawdzać, czy diagram został zaktualizowany
   - Git hooks mogą przypominać o aktualizacji

4. **Wersjonowanie**
   - Trzymaj diagramy w repozytorium
   - Używaj PNG/SVG dla łatwego diffowania w PR

## Dodatkowe zasoby

- [Mermaid Live Editor](https://mermaid.live/)
- [PlantUML Online Server](http://www.plantuml.com/plantuml/)
- [C4 Model](https://c4model.com/) - dla diagramów architektury
