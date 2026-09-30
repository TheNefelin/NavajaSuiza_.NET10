#!/usr/bin/env python3
"""Inyecta la guia de usuario en espanol en el README.

Fuente unica de verdad: NavajaSuiza_.NET10/Resources/Raw/guide/USER_GUIDE.es.md,
que es el mismo archivo que carga la app. El README lo muestra entre dos
marcadores para que no pueda quedar una copia obsoleta dando vueltas.

Solo espanol, a proposito. La app carga USER_GUIDE.{es,en,sv}.md segun el idioma
del dispositivo, pero el README es la documentacion del repositorio y se
mantiene en espanol. Los .en.md y .sv.md no entran aqui.

Adaptaciones al contexto del README:

1. Rutas de imagen. En el .md son relativas al archivo, ![Pizarra](pizarra.jpg).
   En el README se resuelven contra la raiz del repo, donde no existe ningun
   pizarra.jpg, asi que se prefijan con la carpeta de la guia. Las imagenes NO se
   duplican: se enlazan a los originales que ya estan versionados.
2. Encabezados. El .md trae su propio H1 y el README ya tiene uno, asi que se
   baja todo un nivel: H1 pasa a H2, H2 a H3, etc. Los bloques de codigo se
   respetan y no se tocan.

El reemplazo de la region delimitada por los marcadores se hace por indices, no
con una expresion regular: la salida no depende de como estaba escrita la
entrada, asi que correr el script dos veces seguidas no produce diferencias. Eso
es lo que evita que el workflow entre en un ciclo de commits.

No requiere dependencias: solo la biblioteca estandar.
"""

import re
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]
GUIDE_PATH = REPO_ROOT / "NavajaSuiza_.NET10" / "Resources" / "Raw" / "guide" / "USER_GUIDE.es.md"
README_PATH = REPO_ROOT / "README.md"

# Carpeta de la guia tal como se ve desde la raiz del repo. El Workflow corre en
# ubuntu y los enlaces del README se resuelven con barras normales.
GUIDE_DIR = "NavajaSuiza_.NET10/Resources/Raw/guide/"

START_MARKER = "<!-- MANUAL:INICIO - generado por .github/scripts/sync-manual.py, no editar a mano -->"
END_MARKER = "<!-- MANUAL:FIN -->"

# Extension de imagen y todo lo que ya es una ruta utilizable: URL absoluta,
# data URI, ruta absoluta desde la raiz o una que ya tenga el prefijo puesto.
# IGNORECASE para aceptar .JPG junto a .jpg; group() conserva el nombre original.
IMAGE_EXTENSION = r"(?:jpg|jpeg|png|gif|webp)"
SKIP_TARGET = rf"(?!https?://|data:|/|{re.escape(GUIDE_DIR)})"

MD_IMAGE = re.compile(
    rf'(!\[[^\]\n]*\])\({SKIP_TARGET}([^)\s]+\.{IMAGE_EXTENSION})\)',
    re.IGNORECASE,
)
HTML_IMAGE = re.compile(
    rf'(src="){SKIP_TARGET}([^"]+\.{IMAGE_EXTENSION})"',
    re.IGNORECASE,
)

ATX_HEADING = re.compile(r"^(#{1,5})(\s)")


def prefix_images(text: str) -> str:
    """Apunta las imagenes a la carpeta de la guia, sin duplicarlas."""
    text = MD_IMAGE.sub(lambda m: f"{m.group(1)}({GUIDE_DIR}{m.group(2)})", text)
    return HTML_IMAGE.sub(lambda m: f'{m.group(1)}{GUIDE_DIR}{m.group(2)}"', text)


def demote_headings(text: str) -> str:
    """Baja un nivel cada encabezado ATX, sin tocar los bloques de codigo."""
    lines = []
    inside_fence = False

    for line in text.split("\n"):
        if line.lstrip().startswith("```"):
            inside_fence = not inside_fence
            lines.append(line)
            continue

        if not inside_fence and ATX_HEADING.match(line):
            line = "#" + line

        lines.append(line)

    return "\n".join(lines)


def build_body(markdown: str) -> str:
    return demote_headings(prefix_images(markdown)).rstrip()


def replace_region(readme: str, body: str) -> str:
    """Vuelca el cuerpo entre los dos marcadores de forma idempotente."""
    start = readme.index(START_MARKER)
    if start != 0 and readme[start - 1] != "\n":
        raise ValueError("El marcador de inicio debe empezar al principio de una linea.")

    end = readme.index(END_MARKER, start)

    head = readme[: readme.index("\n", start) + 1]
    tail = readme[end:]

    return f"{head}\n{body}\n\n{tail}"


def main() -> int:
    if not GUIDE_PATH.is_file():
        print(f"No se encontro la guia en {GUIDE_PATH}", file=sys.stderr)
        return 1

    # utf-8-sig descarta un eventual BOM; el proyecto usa UTF-8 sin BOM.
    readme = README_PATH.read_text(encoding="utf-8-sig")

    if START_MARKER not in readme or END_MARKER not in readme:
        print(
            "El README no tiene los marcadores de la guia.\n"
            f"Agrega entre el titulo y el resto del README:\n\n{START_MARKER}\n{END_MARKER}",
            file=sys.stderr,
        )
        return 1

    try:
        updated = replace_region(readme, build_body(GUIDE_PATH.read_text(encoding="utf-8-sig")))
    except ValueError as error:
        print(str(error), file=sys.stderr)
        return 1

    if updated == readme:
        print("UNCHANGED")
        return 0

    # newline="\n" evita la traduccion a CRLF que hace Windows por defecto: el
    # repositorio guarda LF.
    with open(README_PATH, "w", encoding="utf-8", newline="\n") as handle:
        handle.write(updated)

    print("CHANGED")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())