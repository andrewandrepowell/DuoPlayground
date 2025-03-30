# -*- coding: utf-8 -*-
"""
Created on Sun Mar 30 11:48:53 2025

@author: andre
"""

import argparse
import typing


if __name__ == "__main__":
    parser = argparse.ArgumentParser(
        prog="GPL_Generator",
        description="Quick script for generating the GPL files Aesprite needs for color palettes.")
    parser.add_argument("--name", help="Palette name. The name in the file itself will be capitalized.")
    parser.add_argument("--description", help="Description of palette.")
    parser.add_argument("--colors", nargs="+", help="List of 3 byte colors in hexacdecimal. For example, 582F0E or 333D29.")
    parsed_arguments = parser.parse_args()
    name = typing.cast(str | None, parsed_arguments.name)
    description = typing.cast(str | None, parsed_arguments.description)
    colors_as_str = typing.cast(list[str] | None, parsed_arguments.colors)
    assert name is not None, "There must be a name specified."
    assert description is not None, "There must be a description specified."
    assert colors_as_str is not None, "There must be colors specified."
    assert len(name) > 0, "name must have at least 1 character."
    assert len(colors_as_str) > 0, "There must be at least 1 color."
    colors = [int(color, 16) for color in colors_as_str]
    assert all(color >= 0x000000 and color <= 0xFFFFFF for color in colors), "All colors should be 3 bytes."
    filename = f"{name.lower()}.gpl"
    with open(filename, "w") as file:
        file.write("GIMP Palette\n")
        file.write(f"#Palette Name: {name.upper()}\n")
        file.write(f"#Description: {description}\n")
        file.write(f"#Colors: {len(colors)}\n")
        for color in colors:
            for component in color.to_bytes(3, "big"):
                file.write(f"{component}\t")
            file.write(f"{hex(color)}\n")
