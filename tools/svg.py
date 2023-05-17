"""
USAGE:
    svg.py [options] <file_or_folder>... <dist>

Options:
    -c, --color=<r,g,b,a>      Color to override. `default` to use original color. [default: 255,255,255,255]
    -s, --size=<widthxheight>  Resize. -1 for auto resize using another size. `default` to use original [default: -1x256]
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
    cur_image = image.convert("RGBA")

    # Create a blank image with the same size and target color
    colored_image = Image.new("RGBA", image.size, color)
    new_data = []
    for old_color in cur_image.getdata():
        if old_color[3] > 0:
            new_data.append(color)
        else:
            new_data.append(old_color)

    colored_image.putdata(new_data)
    return colored_image


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
    color_args = args['--color']
    if color_args == 'default':
        color = None
    else:
        color = tuple(int(each.strip()) for each in args['--color'].split(','))
    # color = (255, 255, 255, 255)
    size_raw = args['--size']
    if size_raw == 'default':
        size_raw = None

    # size_raw = '-1x256'
    dist = args['<dist>']
    if size_raw and 'x' in size_raw:
        width, height = list(map(lambda each: int(each), size_raw.split('x')))
    elif size_raw:
        width = height = int(size_raw)
    else:
        width = height = None
    if width == -1:
        width = None
    if height == -1:
        height = None

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
