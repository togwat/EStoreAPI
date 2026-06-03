import { useRef, useEffect, useState } from "react";
import { useIsMobile } from "@/hooks/use-mobile";

function getAngle(cx: number, cy: number, ex: number, ey: number) {
    const dy = ey - cy;
    const dx = ex - cx;
    const rad = Math.atan2(dy, dx);
    const deg = rad * 180 / Math.PI;
    return deg;
}

export default function Eyes() {
    const rectRef = useRef<HTMLDivElement>(null);
    const [eyeDeg, setEyeDeg] = useState<number | null>(null);
    const isMobile = useIsMobile();

    const scale = isMobile ? 0.5 : 1;

    useEffect(() => {
        document.addEventListener("mousemove", handleMouseMove);
        return () => document.removeEventListener("mousemove", handleMouseMove);
    });

    const handleMouseMove = (event: MouseEvent) => {
        if (!rectRef.current) return null;

        // get pos of eyes and mouse — read live so stale layout never causes offset
        const rect = rectRef.current.getBoundingClientRect();
        const mouseX = event.clientX;
        const mouseY = event.clientY;
        const eyeX = rect.left + rect.width / 2;
        const eyeY = rect.top + rect.height / 2;

        // calculate angle between them
        const angle = getAngle(mouseX, mouseY, eyeX, eyeY);

        // offset angle to compensate for starting position
        // update transform rotate
        setEyeDeg(angle - 45);
    }

    // eye & pupil
    function Eye() {
        return (
            <div
                className="rounded-full border-8 border-foreground bg-transparent w-40 h-40"
                style={{ transform: `rotate(${eyeDeg}deg)` }}>
                <div className="rounded-full bg-foreground w-20 h-20 m-3" />
            </div>
        )
    }

    return (
        <div className="flex flex-row gap-8" style={{ scale: String(scale) }} ref={rectRef}>
            <Eye />
            <Eye />
        </div>
    )
}