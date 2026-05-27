import {
  AttachmentAdapter,
  PendingAttachment,
  CompleteAttachment,
} from "@assistant-ui/react";

/** assistant-ui's SimpleTextAttachmentAdapter does not accept .md files, have to redo it here */

export default class MarkdownAttachmentAdapter implements AttachmentAdapter {
  accept = ".md,.markdown,text/markdown";

  async add({ file }: { file: File }): Promise<PendingAttachment> {
    return {
      id: crypto.randomUUID(),
      type: "document",
      name: file.name,
      contentType: "text/markdown",
      file,
      status: { type: "requires-action", reason: "composer-send" },
    };
  }

  async send(attachment: PendingAttachment): Promise<CompleteAttachment> {
    const text = await attachment.file.text();
    return {
      ...attachment,
      status: { type: "complete" },
      content: [
        {
          type: "text",
          text: `<attachment name="${attachment.name}">\n${text}\n</attachment>`,
        },
      ],
    };
  }

  async remove(): Promise<void> {}
}
