import { useIsMobile } from "@/hooks/use-mobile"

export default function SkillsPage({ title }: { title: string }) {
    const isMobile = useIsMobile();

    return (
        <div>
            <div className={isMobile ? "px-8" : "p-8"}>
                { !isMobile && <h1 className="pb-4">{title}</h1> }
            </div>
        </div>
    )
}