"""
USAGE:
    svg.py [options] <file_or_folder>... <dist>

OPTIONS:
    -c, --color=<color>          override color
    -s, --size=<widthxheight>    resize file, like "256x512". If only use one number, it will be used as both side, like "256" will be the same as "256x256"
"""

import logging
import cairosvg
from PIL import Image


def convert_svg_to_png(svg_path, png_path, width=None, height=None, color=None):
    # Convert SVG to PNG
    logging.debug('save to path %s', png_path)
    cairosvg.svg2png(url=svg_path, write_to=png_path)

    # Open the PNG image
    image = Image.open(png_path)

    # Change color if specified
    if color:
        image = change_image_color(image, color)

    # Resize if width or height is specified
    if width or height:
        image = resize_image(image, width, height)

    # Save the modified image
    image.save(png_path)


def change_image_color(image, color):
    # Convert the image to RGBA mode
    image = image.convert('RGBA')

    # Create a new image with the desired color
    new_image = Image.new('RGBA', image.size, color)

    # Composite the original image with the new color image
    return Image.alpha_composite(new_image, image)


def resize_image(image, width=None, height=None):
    # Calculate the aspect ratio
    aspect_ratio = image.width / image.height

    if width and height:
        # Resize the image using both width and height
        resized_image = image.resize((width, height))
    elif width:
        # Resize the image using only width while maintaining the aspect ratio
        resized_image = image.resize((width, int(width / aspect_ratio)))
    elif height:
        # Resize the image using only height while maintaining the aspect ratio
        resized_image = image.resize((int(height * aspect_ratio), height))

    return resized_image


if __name__ == '__main__':
    import docpie
    import os


    def get_files_under_folder(folder):
        for (root, _, files) in os.walk(folder):
            for each_file in files:
                yield os.path.join(root, each_file)


    logging.basicConfig(level=logging.DEBUG)
    # docpie.logger.setLevel(level=logging.ERROR)
    args = docpie.docpie(__doc__)
    color = args['--color']
    size_raw = args['--size']
    dist = args['<dist>']
    if size_raw and 'x' in size_raw:
        width, height = list(map(size_raw.split('x'), lambda each: int(each)))
    elif size_raw:
        width = height = int(size_raw)
    else:
        width = height = None

    for file_or_folder in args['<file_or_folder>']:
        if os.path.isdir(file_or_folder):
            files = get_files_under_folder(file_or_folder)
        else:
            files = (file_or_folder,)
        
        for each_file in files:
            base_name = os.path.basename(each_file)
            file_name_without_extension = os.path.splitext(base_name)[0]

            if dist.endswith('.png'):
                target_path = dist
            else:
                target_path = os.path.join(dist, f'{file_name_without_extension}.png')

            convert_svg_to_png(each_file, target_path, width, height, color)
