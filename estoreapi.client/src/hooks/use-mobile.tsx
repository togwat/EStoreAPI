import * as React from "react"

const MOBILE_BREAKPOINT = 768

const ContainerIsMobileContext = React.createContext<boolean | null>(null)

// Viewport fallback for components rendered outside any MobileBreakpointProvider.
// Only listens while enabled so container-driven consumers skip the media query.
function useViewportIsMobile(enabled: boolean) {
  const [isMobile, setIsMobile] = React.useState<boolean | undefined>(undefined)

  React.useEffect(() => {
    if (!enabled) return
    const mql = window.matchMedia(`(max-width: ${MOBILE_BREAKPOINT - 1}px)`)
    const onChange = () => {
      setIsMobile(window.innerWidth < MOBILE_BREAKPOINT)
    }
    mql.addEventListener("change", onChange)
    setIsMobile(window.innerWidth < MOBILE_BREAKPOINT)
    return () => mql.removeEventListener("change", onChange)
  }, [enabled])

  return !!isMobile
}

export function useIsMobile() {
  const containerIsMobile = React.useContext(ContainerIsMobileContext)
  const viewportIsMobile = useViewportIsMobile(containerIsMobile === null)
  return containerIsMobile ?? viewportIsMobile
}

// useIsMobile measures this container's width
// Wrap children in this component 
export function IsMobileContainer({
  className,
  children,
}: React.PropsWithChildren<{ className?: string }>) {
  const ref = React.useRef<HTMLDivElement>(null)
  const [isMobile, setIsMobile] = React.useState<boolean | null>(null)

  React.useEffect(() => {
    const el = ref.current
    if (!el) return
    const measure = () => setIsMobile(el.clientWidth < MOBILE_BREAKPOINT)
    const observer = new ResizeObserver(measure)
    observer.observe(el)
    measure()
    return () => observer.disconnect()
  }, [])

  return (
    <ContainerIsMobileContext.Provider value={isMobile}>
      <div ref={ref} className={className}>{children}</div>
    </ContainerIsMobileContext.Provider>
  )
}
