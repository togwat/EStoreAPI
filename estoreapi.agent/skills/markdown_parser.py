import frontmatter


def to_markdown(skill: dict) -> str:
    """
    Converts a dictionary {name, summary, content} into a md file
    Name becomes the file name 'name.md'
    Summary becomes yaml frontmatter 'summary: My summary'
    Content becomes the rest of the md file
    """
    post = frontmatter.Post(skill["content"], summary=skill["summary"])

    return frontmatter.dumps(post)


def from_markdown(name: str, file_path: str) -> dict:
    """
    Converts a md file into a dictionary {name, summary, content}

    name takes the filename
    summary is the yaml frontmatter 'summary:'
    content is the rest of the md file

    Raises ValueError if the file does not match the format.
    """
    try:
        post = frontmatter.loads(file_path)
    except Exception as error:
        # General frontmatter error, usually malformed yaml
        raise ValueError(f"The file's frontmatter could not be read: {error}")

    summary = str(post.get("summary") or "").strip()
    if not summary:
        raise ValueError("The file must start with a '---' frontmatter block containing a non-empty 'summary:' line.")

    return {
        "name": name,
        "summary": summary,
        "content": post.content,
    }
