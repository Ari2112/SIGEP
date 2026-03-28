# 🎨 Paleta de Colores — SIGEP · Centro Agrícola Cantonal Coronado

> Sistema de diseño oficial para el **Sistema Integral de Gestión de Personal (SIGEP)**
> del **Centro Agrícola Cantonal Coronado**.
>
> Extraída directamente del logo corporativo: círculo con degradado verde bosque → verde lima,
> silueta blanca de res y hojas naturales. Evoca **campo, frescura, confianza y sostenibilidad agropecuaria**.

---

## 🌿 Colores Primarios — Extraídos del Logo

| Nombre            | HEX       | RGB                  | Origen en el logo                          |
|-------------------|-----------|----------------------|--------------------------------------------|
| Verde Bosque      | `#2D6A1F` | rgb(45, 106, 31)     | Verde oscuro del fondo superior del círculo |
| Verde Lima        | `#7DC221` | rgb(125, 194, 33)    | Verde brillante del fondo inferior / hojas  |
| Verde Medio       | `#4E9A28` | rgb(78, 154, 40)     | Transición del degradado central            |
| Verde Hoja        | `#A3C41A` | rgb(163, 196, 26)    | Tono amarillo-verde de las hojas grandes    |
| Blanco Logo       | `#FFFFFF` | rgb(255, 255, 255)   | Silueta de la res y contornos de hojas      |

---

## 🌱 Colores Secundarios

| Nombre           | HEX       | RGB                  | Uso principal                              |
|------------------|-----------|----------------------|--------------------------------------------|
| Verde Claro      | `#A8D85E` | rgb(168, 216, 94)    | Hover de botones, tags, badges activos     |
| Verde Menta      | `#D4EDAC` | rgb(212, 237, 172)   | Fondos de cards, inputs deshabilitados     |
| Verde Muy Claro  | `#EFF7DC` | rgb(239, 247, 220)   | Fondos de secciones alternas, tooltips     |

---

## ⚪ Neutros

| Nombre         | HEX       | RGB                  | Uso principal                             |
|----------------|-----------|----------------------|-------------------------------------------|
| Blanco Puro    | `#FFFFFF` | rgb(255, 255, 255)   | Fondo general, texto sobre oscuro, íconos |
| Gris Muy Claro | `#F5F7F2` | rgb(245, 247, 242)   | Fondo de página, contenedores             |
| Gris Claro     | `#E2E8D9` | rgb(226, 232, 217)   | Bordes de cards, divisores                |
| Gris Medio     | `#8A9E7C` | rgb(138, 158, 124)   | Texto secundario, placeholders            |
| Gris Oscuro    | `#3D4D35` | rgb(61, 77, 53)      | Texto principal sobre fondos claros       |
| Negro Suave    | `#1C2A16` | rgb(28, 42, 22)      | Títulos, texto de alto contraste          |

---

## 🔴 Colores de Estado / Semánticos

| Nombre           | HEX       | Uso                                          |
|------------------|-----------|----------------------------------------------|
| Éxito / Success  | `#4CAF50` | Confirmaciones, validaciones OK              |
| Advertencia      | `#F9A825` | Alertas menores, campos con aviso            |
| Error / Danger   | `#D32F2F` | Errores de formulario, acciones destructivas |
| Info             | `#1976D2` | Notificaciones informativas, tooltips        |

---

## 🔤 Sistema Tipográfico

### Familias de Fuentes

| Rol               | Fuente              | Google Fonts URL param                            | Uso                                         |
|-------------------|---------------------|---------------------------------------------------|---------------------------------------------|
| Display / Marca   | **Playfair Display** | `family=Playfair+Display:wght@600;700;800`       | Hero titles, nombre del sistema, portada    |
| Encabezados       | **Nunito**           | `family=Nunito:wght@400;500;600;700;800`         | H1–H4, subtítulos, navegación               |
| Cuerpo / UI       | **Source Sans 3**    | `family=Source+Sans+3:wght@300;400;500;600`      | Párrafos, labels, inputs, tablas            |
| Monoespaciada     | **JetBrains Mono**   | `family=JetBrains+Mono:wght@400;500`             | Códigos, IDs, datos técnicos, trazabilidad  |

> **Racional**: Playfair Display aporta elegancia orgánica y autoridad de marca (evoca tradición agropecuaria).
> Nunito es amigable y redondo, ideal para una app de campo accesible. Source Sans 3 garantiza máxima
> legibilidad en pantallas. JetBrains Mono para datos técnicos como códigos de empleado, planillas, etc.

### Importación en HTML

```html
<link rel="preconnect" href="https://fonts.googleapis.com">
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
<link href="https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;500&family=Nunito:wght@400;500;600;700;800&family=Playfair+Display:wght@600;700;800&family=Source+Sans+3:wght@300;400;500;600&display=swap" rel="stylesheet">
```

### Escala Tipográfica

| Token            | Fuente            | Tamaño  | Peso  | Line-height | Uso                           |
|------------------|-------------------|---------|-------|-------------|-------------------------------|
| `--text-display` | Playfair Display  | 48px    | 700   | 1.15        | Hero titles, portada          |
| `--text-h1`      | Playfair Display  | 36px    | 600   | 1.2         | Títulos principales de página |
| `--text-h2`      | Nunito            | 28px    | 700   | 1.3         | Títulos de sección            |
| `--text-h3`      | Nunito            | 22px    | 600   | 1.35        | Subtítulos, card headers      |
| `--text-h4`      | Nunito            | 18px    | 600   | 1.4         | Labels de grupo, accordions   |
| `--text-body-lg` | Source Sans 3     | 17px    | 400   | 1.6         | Párrafos destacados           |
| `--text-body`    | Source Sans 3     | 15px    | 400   | 1.6         | Texto general de la app       |
| `--text-body-sm` | Source Sans 3     | 13px    | 400   | 1.5         | Metadatos, fechas, captions   |
| `--text-label`   | Nunito            | 13px    | 600   | 1.4         | Labels de formulario, tags    |
| `--text-caption` | Source Sans 3     | 12px    | 300   | 1.4         | Notas al pie, tooltips        |
| `--text-button`  | Nunito            | 14px    | 700   | 1           | Texto de botones              |
| `--text-nav`     | Nunito            | 14px    | 600   | 1           | Ítems de navegación           |
| `--text-code`    | JetBrains Mono    | 13px    | 400   | 1.5         | Códigos, IDs, datos técnicos  |

---

## 🖱️ Botones

### Primario
```
Fondo:         #7DC221  (Verde Lima)
Texto:         #FFFFFF
Hover fondo:   #4E9A28  (Verde Medio)
Border-radius: 8px
Sombra:        0 2px 8px rgba(125, 194, 33, 0.35)
```

### Secundario (Outline)
```
Fondo:         transparent
Texto:         #4E9A28
Borde:         2px solid #4E9A28
Hover fondo:   #EFF7DC
Border-radius: 8px
```

### Destructivo
```
Fondo:         #D32F2F
Texto:         #FFFFFF
Hover fondo:   #B71C1C
Border-radius: 8px
```

### Deshabilitado
```
Fondo:         #E2E8D9
Texto:         #8A9E7C
Cursor:        not-allowed
```

---

## 🃏 Cards

```
Fondo:           #FFFFFF
Borde:           1px solid #E2E8D9
Border-radius:   12px
Sombra:          0 2px 12px rgba(45, 106, 31, 0.08)
Padding:         24px

— Header de card destacado —
Fondo header:    #EFF7DC
Texto header:    #2D6A1F
Border-bottom:   1px solid #D4EDAC

— Card activa / seleccionada —
Borde:           2px solid #7DC221
Sombra:          0 4px 16px rgba(125, 194, 33, 0.20)

— Card hover —
Sombra:          0 6px 20px rgba(45, 106, 31, 0.14)
Transform:       translateY(-2px)
```

---

## 📝 Inputs y Formularios

```
Fondo:            #FFFFFF
Borde normal:     1px solid #E2E8D9
Borde focus:      2px solid #7DC221
Borde error:      2px solid #D32F2F
Borde éxito:      2px solid #4CAF50
Border-radius:    8px
Texto:            #3D4D35
Placeholder:      #8A9E7C
Label:            #3D4D35  (font-weight: 500)
Fondo disabled:   #F5F7F2
Focus shadow:     0 0 0 3px rgba(125, 194, 33, 0.20)
```

---

## 🔔 Notificaciones / Alerts

```
— Éxito —
Fondo: #EFF7DC    Borde izq: 4px solid #4CAF50    Texto: #2D6A1F

— Advertencia —
Fondo: #FFF8E1    Borde izq: 4px solid #F9A825    Texto: #5D4037

— Error —
Fondo: #FFEBEE    Borde izq: 4px solid #D32F2F    Texto: #B71C1C

— Info —
Fondo: #E3F2FD    Borde izq: 4px solid #1976D2    Texto: #0D47A1
```

---

## 🗂️ Navegación / Sidebar

```
Fondo sidebar:          #2D6A1F  (Verde Bosque)
Texto ítem:             #D4EDAC
Ícono ítem:             #A8D85E
Ítem activo fondo:      rgba(125, 194, 33, 0.25)
Ítem activo borde izq:  4px solid #7DC221
Ítem activo texto:      #FFFFFF
Ítem hover fondo:       rgba(255, 255, 255, 0.08)
Header nav (topbar):    #FFFFFF  con sombra: 0 1px 4px rgba(45,106,31,0.12)
Logo en sidebar:        mostrar logoSIGEP.jpeg + texto "SIGEP" en Playfair Display
```

---

## 🏷️ Badges / Tags

```
— Verde (activo, aprobado) —
Fondo: #D4EDAC    Texto: #2D6A1F    Border-radius: 999px    Padding: 2px 10px

— Amarillo (pendiente) —
Fondo: #FFF8C5    Texto: #7A5F00

— Rojo (inactivo, rechazado) —
Fondo: #FFEBEE    Texto: #C62828

— Gris (neutro, borrador) —
Fondo: #E2E8D9    Texto: #4D5D44
```

---

## 📊 Tablas

```
Cabecera (thead):   Fondo #2D6A1F    Texto #FFFFFF    Font-weight: 600
Fila par:           Fondo #F5F7F2
Fila impar:         Fondo #FFFFFF
Fila hover:         Fondo #EFF7DC
Borde celda:        1px solid #E2E8D9
Texto celdas:       #3D4D35
```

---

## 🌊 Degradados de Marca

```css
/* Degradado principal (idéntico al logo: bosque → lima) */
background: linear-gradient(135deg, #2D6A1F 0%, #7DC221 100%);

/* Hero banner / bienvenida */
background: linear-gradient(135deg, #2D6A1F 0%, #4E9A28 50%, #7DC221 100%);

/* Degradado suave para secciones alternas */
background: linear-gradient(180deg, #EFF7DC 0%, #FFFFFF 100%);

/* Botones CTA especiales */
background: linear-gradient(90deg, #4E9A28 0%, #7DC221 100%);
```

---

## 🔲 Sombras Estándar

```css
--shadow-sm:   0 1px 4px rgba(45, 106, 31, 0.08);
--shadow-md:   0 2px 12px rgba(45, 106, 31, 0.12);
--shadow-lg:   0 6px 24px rgba(45, 106, 31, 0.16);
--shadow-xl:   0 12px 40px rgba(45, 106, 31, 0.22);
```

---

## 🎨 CSS Variables — Design Tokens Completos

```css
@import url('https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;500&family=Nunito:wght@400;500;600;700;800&family=Playfair+Display:wght@600;700;800&family=Source+Sans+3:wght@300;400;500;600&display=swap');

:root {
  /* ── COLORES PRIMARIOS ── */
  --color-primary:          #7DC221;   /* Verde Lima — CTA, botones, activos */
  --color-primary-dark:     #4E9A28;   /* Verde Medio — hover, degradados */
  --color-primary-darker:   #2D6A1F;   /* Verde Bosque — sidebar, headers */
  --color-primary-leaf:     #A3C41A;   /* Verde Hoja — acento amarillo-verde */

  /* ── COLORES SECUNDARIOS ── */
  --color-secondary:        #A8D85E;   /* Verde Claro — hover suave, tags */
  --color-secondary-light:  #D4EDAC;   /* Verde Menta — fondos de card */
  --color-bg-alt:           #EFF7DC;   /* Verde Muy Claro — secciones alternas */

  /* ── NEUTROS ── */
  --color-bg:               #F5F7F2;   /* Fondo de página */
  --color-surface:          #FFFFFF;   /* Cards, modales, inputs */
  --color-border:           #E2E8D9;   /* Bordes sutiles */
  --color-text:             #3D4D35;   /* Texto principal */
  --color-text-secondary:   #8A9E7C;   /* Texto secundario / placeholder */
  --color-text-strong:      #1C2A16;   /* Títulos de alto contraste */

  /* ── SEMÁNTICOS ── */
  --color-success:          #4CAF50;
  --color-warning:          #F9A825;
  --color-error:            #D32F2F;
  --color-info:             #1976D2;

  /* ── DEGRADADOS ── */
  --gradient-brand:         linear-gradient(135deg, #2D6A1F 0%, #7DC221 100%);
  --gradient-hero:          linear-gradient(135deg, #2D6A1F 0%, #4E9A28 50%, #7DC221 100%);
  --gradient-soft:          linear-gradient(180deg, #EFF7DC 0%, #FFFFFF 100%);
  --gradient-button:        linear-gradient(90deg, #4E9A28 0%, #7DC221 100%);

  /* ── SOMBRAS ── */
  --shadow-sm:              0 1px 4px rgba(45, 106, 31, 0.08);
  --shadow-md:              0 2px 12px rgba(45, 106, 31, 0.12);
  --shadow-lg:              0 6px 24px rgba(45, 106, 31, 0.16);
  --shadow-xl:              0 12px 40px rgba(45, 106, 31, 0.22);

  /* ── BORDER RADIUS ── */
  --radius-sm:              4px;
  --radius-md:              8px;
  --radius-lg:              12px;
  --radius-xl:              16px;
  --radius-full:            999px;

  /* ── TIPOGRAFÍA — FAMILIAS ── */
  --font-display:           'Playfair Display', Georgia, serif;
  --font-heading:           'Nunito', 'Segoe UI', sans-serif;
  --font-body:              'Source Sans 3', 'Helvetica Neue', sans-serif;
  --font-mono:              'JetBrains Mono', 'Courier New', monospace;

  /* ── TIPOGRAFÍA — TAMAÑOS ── */
  --text-display:           3rem;       /* 48px */
  --text-h1:                2.25rem;    /* 36px */
  --text-h2:                1.75rem;    /* 28px */
  --text-h3:                1.375rem;   /* 22px */
  --text-h4:                1.125rem;   /* 18px */
  --text-body-lg:           1.0625rem;  /* 17px */
  --text-body:              0.9375rem;  /* 15px */
  --text-body-sm:           0.8125rem;  /* 13px */
  --text-label:             0.8125rem;  /* 13px */
  --text-caption:           0.75rem;    /* 12px */
  --text-button:            0.875rem;   /* 14px */
  --text-nav:               0.875rem;   /* 14px */
  --text-code:              0.8125rem;  /* 13px */

  /* ── TIPOGRAFÍA — PESOS ── */
  --font-light:             300;
  --font-regular:           400;
  --font-medium:            500;
  --font-semibold:          600;
  --font-bold:              700;
  --font-extrabold:         800;

  /* ── LINE HEIGHTS ── */
  --leading-tight:          1.2;
  --leading-snug:           1.35;
  --leading-normal:         1.5;
  --leading-relaxed:        1.6;
}
```

---

## 📐 Aplicación por Componente — CSS de Referencia

```css
/* ── Estructura base ── */
body {
  font-family: var(--font-body);
  background: var(--color-bg);
  color: var(--color-text);
  font-size: var(--text-body);
  line-height: var(--leading-relaxed);
}

/* ── Títulos ── */
h1 { font-family: var(--font-display); font-size: var(--text-h1); font-weight: var(--font-semibold); color: var(--color-text-strong); }
h2 { font-family: var(--font-heading); font-size: var(--text-h2); font-weight: var(--font-bold);     color: var(--color-text); }
h3 { font-family: var(--font-heading); font-size: var(--text-h3); font-weight: var(--font-semibold); color: var(--color-text); }
h4 { font-family: var(--font-heading); font-size: var(--text-h4); font-weight: var(--font-semibold); color: var(--color-text); }

/* ── Sidebar ── */
.sidebar {
  background: var(--color-primary-darker);   /* #2D6A1F */
  color: var(--color-secondary-light);
}
.sidebar .nav-item.active {
  background: rgba(125, 194, 33, 0.25);
  border-left: 4px solid var(--color-primary);
  color: #FFFFFF;
}
.sidebar .nav-item:hover {
  background: rgba(255, 255, 255, 0.08);
}

/* ── Topbar ── */
.topbar {
  background: var(--color-surface);
  box-shadow: var(--shadow-sm);
  border-bottom: 1px solid var(--color-border);
}

/* ── Hero banner (reemplaza el degradado morado) ── */
.hero-banner {
  background: var(--gradient-hero);
  color: #FFFFFF;
  border-radius: var(--radius-lg);
  padding: 28px 32px;
}

/* ── Cards de acceso rápido ── */
.quick-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
  transition: box-shadow 0.2s, transform 0.2s;
}
.quick-card:hover {
  box-shadow: var(--shadow-md);
  transform: translateY(-2px);
  border-color: var(--color-primary);
}

/* ── Botón primario ── */
.btn-primary {
  background: var(--gradient-button);
  color: #FFFFFF;
  font-family: var(--font-heading);
  font-size: var(--text-button);
  font-weight: var(--font-bold);
  border: none;
  border-radius: var(--radius-md);
  box-shadow: 0 2px 8px rgba(125, 194, 33, 0.35);
  letter-spacing: 0.04em;
  cursor: pointer;
  transition: filter 0.2s;
}
.btn-primary:hover { filter: brightness(1.08); }

/* ── Inputs ── */
input, select, textarea {
  font-family: var(--font-body);
  font-size: var(--text-body);
  color: var(--color-text);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
}
input:focus, select:focus, textarea:focus {
  outline: none;
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px rgba(125, 194, 33, 0.20);
}

/* ── Código / IDs ── */
code, .id-tag {
  font-family: var(--font-mono);
  font-size: var(--text-code);
  background: var(--color-bg-alt);
  color: var(--color-primary-darker);
  padding: 2px 6px;
  border-radius: var(--radius-sm);
}
```

---

## 🖼️ Identidad de Marca en la UI

- **Logo**: `logoSIGEP.jpeg` — círculo con degradado verde bosque→lima, res blanca y hojas
- **Nombre del sistema**: `SIGEP` — en **Playfair Display 700**, color `#2D6A1F`
- **Subtítulo**: `Sistema Integral de Gestión de Personal` — en **Source Sans 3 400**, color `#8A9E7C`
- **Organización**: `Centro Agrícola Cantonal Coronado` — en **Nunito 600**, color `#3D4D35`
- **Posición del logo en sidebar**: logo circular 40px + "SIGEP" a la derecha, sobre fondo `#2D6A1F`
- **Login page**: mostrar logo centrado (80–96px), degradado de fondo `var(--gradient-hero)` o fondo `#EFF7DC`

---

*Paleta generada a partir del logo corporativo del Centro Agrícola Cantonal Coronado — verde natural, ganado, campo sostenible.*
*Versión 2.0 — Marzo 2026*
