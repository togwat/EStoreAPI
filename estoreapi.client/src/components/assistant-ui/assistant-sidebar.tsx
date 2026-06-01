import {
  ResizableHandle,
  ResizablePanel,
  ResizablePanelGroup,
} from "src/components/ui/resizable";
import type { FC, PropsWithChildren } from "react";

import { Thread } from "src/components/assistant-ui/thread";

import { useIsMobile } from "@/hooks/use-mobile";

const defaultChatWidth = 25;
const defaultChatWidthMobile = 50;

export const AssistantSidebar: FC<PropsWithChildren> = ({ children }) => {
  const isMobile = useIsMobile();
  return (
    <ResizablePanelGroup key={isMobile ? "vertical" : "horizontal"} orientation={isMobile ? "vertical" : "horizontal"}>
      <ResizablePanel className="" defaultSize={isMobile ? 100 - defaultChatWidthMobile : 100 - defaultChatWidth}>
        <div className="min-w-md h-full relative">{children}</div>
      </ResizablePanel>
      <ResizableHandle withHandle />
      <ResizablePanel defaultSize={isMobile ? defaultChatWidthMobile : defaultChatWidth}>
        <div className="min-w-md h-full"><Thread /></div>
      </ResizablePanel>
    </ResizablePanelGroup>
  );
};
