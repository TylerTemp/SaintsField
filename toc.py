from __future__ import annotations
import sys
import os
import json
import dataclasses
from dataclasses import dataclass
import re
from typing import Iterator

import colorlog
import pprint
import logging


handler = colorlog.StreamHandler()
handler.setFormatter(colorlog.ColoredFormatter(
	'%(log_color)s[%(levelname)s:%(name)s] %(message)s'))

logger = colorlog.getLogger('read_me_parser')
logger.addHandler(handler)
# logging.basicConfig(level=logging.DEBUG)
logger.setLevel(logging.DEBUG)

class EnhancedJSONEncoder(json.JSONEncoder):
    def default(self, o):
        if dataclasses.is_dataclass(o):
            return dataclasses.asdict(o)
        return super().default(o)


@dataclass(frozen=True)
class TitleAndContent:
    title: str
    title_id: str
    title_level: int
    content: list[str]
    sub_content: list[TitleAndContent]


readme_path = os.path.join(os.path.dirname(__file__), 'Packages', 'today.comes.saintsfield', 'Docs~', 'README.md')

root_title: TitleAndContent | None = None
title_chain: list[TitleAndContent] = []

with open(readme_path, 'r', encoding='utf-8') as f:
    for line in f:
        if line.startswith('#') and not line.startswith('#if ') and not line.startswith('#endif'):
            split_header_md = line.split()
            split_header_md.pop(len(split_header_md) - 1)
            title_start = split_header_md.pop(0)
            title_level = len(title_start)
            title = ' '.join(split_header_md)
            title_id: str = re.sub(r'[^a-zA-Z\s\-]', '', title).replace(' ', '-').lower()

            if title_level == 1:
                root_title = TitleAndContent(title, title_id, title_level, [], [])
                title_chain.append(root_title)
            else:
                last_title = title_chain[-1]
                if last_title.title_level >= title_level:
                    while title_chain[-1].title_level >= title_level:
                        title_chain.pop()
                new_title = TitleAndContent(title, title_id, title_level, [], [])
                title_chain[-1].sub_content.append(new_title)
                title_chain.append(new_title)
        else:
            title_chain[-1].content.append(line)

# print(json.dumps(root_title, cls=EnhancedJSONEncoder, indent=4))
@dataclass()
class TitleAndContentCompact:
    Title: str
    TitleId: str
    TitleLevel: int
    Content: str
    SubContents: list[TitleAndContentCompact]


def compact_title_and_content(title_and_content: TitleAndContent) -> TitleAndContentCompact:
    return TitleAndContentCompact(
        Title=title_and_content.title,
        TitleId=title_and_content.title_id,
        TitleLevel=title_and_content.title_level,
        Content=''.join(title_and_content.content).strip(),
        SubContents=[compact_title_and_content(sub) for sub in title_and_content.sub_content]
    )

root_compact = compact_title_and_content(root_title)

list_compact: list[TitleAndContentCompact] = []
root_node = root_compact
root_node.TitleId = ""
sub_list = list(root_compact.SubContents)
root_compact.SubContents.clear()

list_compact.append(root_node)
list_compact.extend(sub_list)

if list_compact[0].Title == 'SaintsField':
    list_compact[0].Title = ''

# markdown_link_pattern = re.compile(r'\[([^\]]+)\]\(([^)]+)\)')
markdown_image_pattern = re.compile(r'!\[([^\]]*)\]\(([^)]+)\)')

def make_id(root: str, title_id: str) -> str:
    if title_id == '':
        return '/'
    return f'{root}/{title_id}'

def make_linked_item(item: TitleAndContentCompact, root: str, resource_folder: str) -> TitleAndContentCompact:
    raw_content: str = item.Content
    # linked_content = convert_link(raw_content, root)

    # logger.debug(item.TitleId)

    # sub_id: str = make_id(root, item.TitleId)
    # logger.debug(f'Processing linked item id: {sub_id}')

    linked_content = raw_content
    sub_contents: list[TitleAndContentCompact] = []
    for sub in item.SubContents:
        sub_contents.append(make_linked_item(sub, make_id(root, sub.TitleId), f'{resource_folder}/{sub.TitleId}'))

    return TitleAndContentCompact(
        Title=item.Title,
        TitleId=item.TitleId,
        TitleLevel=item.TitleLevel,
        Content=linked_content,
        SubContents=sub_contents
    )


def make_linked_list(list_compact: list[TitleAndContentCompact], root: str, project_folder: str):
    for item in list_compact:
        title_id: str = item.TitleId
        resource_id: str = make_id(root, title_id)
        logger.debug(f'Processing id: {resource_id}')
        resource_folder: str = os.path.join(project_folder, title_id)
        yield make_linked_item(item, resource_id, resource_folder)

# pprint.pprint(linked_lis)
# print(len(linked_lis))
# print(json.dumps(linked_lis, cls=EnhancedJSONEncoder, indent=2))

def make_target_link(content: str, url: str) -> str:
    return f'<a href="{url}" target="_blank">{content}</a>'

def gen_sub_links(root: str, indent: int, sub_contents: list[TitleAndContentCompact]):
    for item in sub_contents:
        if item.Content == '' or item.Content is None:
            yield f'{" " * (indent * 4)}*   {item.Title}'
        else:
            yield f'{" " * (indent * 4)}*   ' + make_target_link(item.Title, f"https://saintsfield.comes.today/{root}/{item.TitleId}")
        if len(item.SubContents) > 0:
            for sub in gen_sub_links(f'{root}/{item.TitleId}', indent + 1, item.SubContents):
                yield sub

def gen_links(linked_list: list[TitleAndContentCompact]):
    for item in linked_list:
        title_id: str = item.TitleId
        if title_id == '' or title_id == 'getting-started' or title_id == 'donation':
            continue

        title_str: str = item.Title
        logger.debug(f'Title id: {title_id} ({title_str})')
        # yield f'*   ' + make_target_link(title_str, f'https://saintsfield.comes.today/{title_id}')

        if item.Content == '' or item.Content is None:
            yield f'*   {item.Title}'
        else:
            yield f'*   ' + make_target_link(item.Title, f"https://saintsfield.comes.today/{item.TitleId}")

        for title_md in gen_sub_links(title_id, 1, item.SubContents):
            yield title_md



proj_folder = os.path.normpath(os.path.join(__file__, '..', '..'))
linked_lis = list(make_linked_list(list_compact, '', os.path.join(proj_folder, 'src', 'Assets')))


def get_first_change_log(changelog):
    second_level_counter: int = 0
    for line in changelog:
        if line.startswith('## '):
            second_level_counter += 1
            if second_level_counter >= 2:
                return
            continue

        if second_level_counter == 1:
            yield line

with open(readme_path, 'r', encoding='utf-8') as source_readme:
    with open(os.path.join(os.path.dirname(__file__), 'Packages', 'today.comes.saintsfield', 'README.md'), 'w', encoding='utf-8') as main_readme:

        for line in source_readme:
            if line.startswith('## Getting Started ##'):
                with open(os.path.join(os.path.dirname(__file__), 'Packages', 'today.comes.saintsfield', 'Docs~', 'ReadMeMainFragment.md'), 'r', encoding='utf-8') as fragment:
                    main_readme.write(fragment.read())
                    main_readme.write('\n')

            if line.startswith('### Change Log ###'):

                main_readme.write(line)

                with open(os.path.join(os.path.dirname(__file__), 'Packages', 'today.comes.saintsfield', 'CHANGELOG.md'), 'r',
                          encoding='utf-8') as changelog:
                    main_readme.writelines(get_first_change_log(changelog))
                break
            main_readme.write(line)

        main_readme.write('## Usage ##\n\n')
        for link in gen_links(linked_lis):
            main_readme.write(link)
            main_readme.write('\n')
        main_readme.write('\n')

        found_left: bool = False
        for line in source_readme:
            if line.startswith('## Donation ##'):
                found_left = True
            if found_left:
                main_readme.write(line)
