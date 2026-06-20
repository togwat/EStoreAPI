import {
  ResizableHandle,
  ResizablePanel,
  ResizablePanelGroup,
} from "src/components/ui/resizable";
import type { FC, PropsWithChildren } from "react";
import { useCallback, useMemo } from "react";

import { Thread } from "src/components/assistant-ui/thread";

import { useIsMobile } from "@/hooks/use-mobile";

const defaultChatWidth = 25;
const defaultChatWidthMobile = 50;

// cookies for remembering set width, rather than resetting every reload
const COOKIE_DESKTOP = "chat-width-desktop";
const COOKIE_MOBILE = "chat-width-mobile";  // actually height but use width for consistency

function readChatWidthCookie(key: string): number | null {
  const match = document.cookie.match(new RegExp(`(?:^|; )${key}=([^;]*)`));
  const parsed = match ? parseFloat(match[1]) : NaN;
  if (isNaN(parsed)) return null;
  // refresh expiry on each page load so it only expires after a year of inactivity
  writeChatWidthCookie(key, parsed);
  return parsed;
}

function writeChatWidthCookie(key: string, value: number) {
  // 1 year persistence
  const expires = new Date();
  expires.setFullYear(expires.getFullYear() + 1);
  document.cookie = `${key}=${value}; expires=${expires.toUTCString()}; path=/`;
}

export const AssistantSidebar: FC<PropsWithChildren> = ({ children }) => {
  const isMobile = useIsMobile();
  const cookieKey = isMobile ? COOKIE_MOBILE : COOKIE_DESKTOP;
  const fallback = isMobile ? defaultChatWidthMobile : defaultChatWidth;

  // Read persisted width from cookie, with fallback to default
  const chatWidth = useMemo(
    () => readChatWidthCookie(cookieKey) ?? fallback,
    [cookieKey, fallback]
  );

  // on resize, write to the cookie the new width
  const handleChatResize = useCallback(
    ({ asPercentage }: { asPercentage: number }) => writeChatWidthCookie(cookieKey, asPercentage),
    [cookieKey]
  );

  return (
    <ResizablePanelGroup key={isMobile ? "vertical" : "horizontal"} orientation={isMobile ? "vertical" : "horizontal"}>
      <ResizablePanel className={"!overflow-clip"} defaultSize={100 - chatWidth}>
        <div className={`${isMobile ? "min-w-0" : "min-w-sm"} h-full relative`}>{children}</div>
      </ResizablePanel>
      <ResizableHandle withHandle />
      <ResizablePanel defaultSize={chatWidth} onResize={handleChatResize}>
        <div className={`${isMobile ? "min-w-0" : "min-w-sm"} h-full`}><Thread /></div>
      </ResizablePanel>
    </ResizablePanelGroup>
  );
};
