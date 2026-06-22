import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "src/components/ui/popover";
import { ThreadList } from "src/components/assistant-ui/thread-list";
import { Button } from "src/components/ui/button";
import { ThreadListPrimitive, useAuiState } from "@assistant-ui/react";
import { HistoryIcon, MessageCirclePlusIcon } from "lucide-react";
import type { FC } from "react";

/**
 * Top bar of the chat window: the active chat's title, a history button that opens the saved-session list, and a new button that creates a new session
 * This component is custom-made rather than imported
 */
export const ChatHeader: FC = () => {
  const title = useAuiState((s) => s.threadListItem.title);

  return (
    <div className="aui-chat-header flex items-center justify-between border-b border-border px-3 py-2">
      <span className="font-medium">
        {title || "New chat"}
      </span>

      <div className="flex items-center gap-2">
        <Popover>
          <PopoverTrigger asChild>
            <Button variant="ghost" size="icon" title="Chat history" aria-label="Chat history">
              <HistoryIcon />
            </Button>
          </PopoverTrigger>
          <PopoverContent align="end">
            <ThreadList />
          </PopoverContent>
        </Popover>

        <ThreadListPrimitive.New asChild>
          <Button variant="ghost" size="icon" title="New chat" aria-label="New chat">
            <MessageCirclePlusIcon />
          </Button>
        </ThreadListPrimitive.New>
      </div>
    </div>
  );
};