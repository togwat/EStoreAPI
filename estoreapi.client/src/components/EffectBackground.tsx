import { useTheme } from "@/lib/theme"
import { themeEffectBackgrounds } from "@/lib/themeMapping"
import { ReactNode, useMemo, useRef, useEffect, RefObject } from "react"
import type { PatternEffect } from "@/lib/effectPatterns"
import { cn } from "src/lib/utils"

// effects engine
function usePatternEffect<TParticle>(
    containerRef: RefObject<HTMLElement | null>,
    canvasRef: RefObject<HTMLCanvasElement | null>,
    effect: PatternEffect<TParticle>,
) {
    useEffect(() => {
        const container = containerRef.current;
        const canvas = canvasRef.current;
        if (!container || !canvas) return;

        const ctx = canvas.getContext("2d");
        if (!ctx) return;

        // cap dpr at 2 so retina displays don't quadruple the fill cost
        const dpr = Math.min(window.devicePixelRatio || 1, 2);
        let width = 0;
        let height = 0;
        let t = 0;
        let rafId = 0;
        let particles: TParticle[] = [];

        // swapping themes recolours the effect without touching any effect code.
        function getColour(): string {
            const style = getComputedStyle(container!);
            return (
                style.getPropertyValue("--bg-effect-color").trim() ||
                style.getPropertyValue("--primary").trim() ||
                "#ff00ff" // TEXTURE NOT FOUND
            );
        }

        function resize() {
            const rect = container!.getBoundingClientRect();
            width = rect.width;
            height = rect.height;
            canvas!.width = width * dpr;
            canvas!.height = height * dpr;
            ctx!.setTransform(dpr, 0, 0, dpr, 0, 0);
            if (particles.length === 0) {
                particles = effect.seed(width, height);
            } else if (effect.onResize) {
                particles = effect.onResize(particles, width, height);
            }
        }

        function draw() {
            rafId = requestAnimationFrame(draw);
            t += 0.01;
            ctx!.clearRect(0, 0, width, height);
            effect.frame(ctx!, particles, {
                width,
                height,
                colour: getColour(),
                sizeMultiplier: 1,
                t,
            });
            ctx!.globalAlpha = 1;
        }

        resize();
        draw();

        const resizeObserver = new ResizeObserver(resize);
        resizeObserver.observe(container);

        // React's own cleanup stops the loop + observer when effect/theme changes.
        return () => {
            cancelAnimationFrame(rafId);
            resizeObserver.disconnect();
        };
    }, [containerRef, canvasRef, effect]);
}

interface EffectsBackgroundProps {
    children: ReactNode
    className?: string
}

export function EffectBackground({children, className}: EffectsBackgroundProps) {
    const containerRef = useRef<HTMLDivElement>(null);
    const canvasRef = useRef<HTMLCanvasElement>(null);

    const theme = useTheme();
    // Rebuild the effect only when the theme changes
    const effect = useMemo(() => themeEffectBackgrounds[theme](), [theme]);

    usePatternEffect(containerRef, canvasRef, effect);

    return (
        <div className={cn("relative z-0", className)} ref={containerRef}>
            <canvas className="pointer-events-none absolute inset-0 z-0 h-full w-full" ref={canvasRef} />
            {children}
        </div>
    )
}