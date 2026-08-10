import { useIsMobile } from "@/hooks/use-mobile";
import { createSkill, deleteSkill, getSkill, listSkills, Skill, SkillSummary, updateSkill } from "@/api/skills";
import SkillCard from "./components/SkillCard";
import NewSkillCard from "./components/NewSkillCard";
import { ChangeEvent, useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Dialog, DialogClose, DialogContent, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { ArrowLeft, DownloadIcon, PencilIcon, Trash2Icon, UploadIcon } from "lucide-react";
import { cn } from "@/lib/utils";
import Markdown, { type Components } from "react-markdown";
import remarkGfm from "remark-gfm";
import remarkFrontmatter from "remark-frontmatter";
import { downloadTextFile } from "@/lib/downloadTextFile";

// shared styles between markdown doc view and edit mode
const containerStyle = "border bg-input rounded-xl p-4 w-full my-2 min-w-0 min-h-[60vh]";
const markdownStyle = "[&_h1]:mb-2 [&_h1]:text-2xl [&_h1]:font-semibold [&_h2]:mt-6 [&_h2]:mb-2 [&_h2]:text-xl [&_h2]:font-semibold [&_h3]:mt-4 [&_h3]:mb-1 [&_h3]:text-lg [&_h3]:font-medium [&_p]:my-3 [&_ul]:my-3 [&_ul]:ml-6 [&_ul]:list-disc [&_ol]:my-3 [&_ol]:ml-6 [&_ol]:list-decimal [&_li]:my-1 [&_a]:text-primary [&_a]:underline [&_code]:rounded [&_code]:bg-muted [&_code]:px-1 [&_code]:py-0.5 [&_code]:text-sm [&_pre]:my-3 [&_pre]:overflow-x-auto [&_pre]:rounded-lg [&_pre]:bg-muted [&_pre]:p-3 [&_blockquote]:border-l-2 [&_blockquote]:pl-3 [&_blockquote]:text-muted-foreground [&_table]:my-3 [&_table]:w-max [&_table]:min-w-full [&_table]:border-collapse [&_table]:text-sm [&_th]:border [&_th]:border-border [&_th]:bg-muted [&_th]:px-2 [&_th]:py-1 [&_th]:text-start [&_th]:font-medium [&_td]:border [&_td]:border-border [&_td]:px-2 [&_td]:py-1 [&_td]:text-start";

// give a table its own scroll box
const markdownComponents: Components = {
    table: ({ node, ...props }) => (
        <div className="overflow-x-auto">
            <table {...props} />
        </div>
    ),
};

const newSkillTemplate = `---
summary: One sentence on what this skill does and when to use it.
---

Content here...
`;

export default function SkillsPage({ title }: { title: string }) {
    const isMobile = useIsMobile();
    const [summaries, setSummaries] = useState<SkillSummary[]>([]);
    const [selectedSummary, setSelectedSummary] = useState<SkillSummary | null>(null);
    const [selectedSkill, setSelectedSkill] = useState<Skill | null>(null);
    // mode switching
    type Modes =
        | 'view'
        | 'edit'
        | 'create'
    const [currentMode, setCurrentMode] = useState<Modes>('view');
    // unsaved edits, kept apart from selectedSkill so cancelling restores the saved file
    const [editedName, setEditedName] = useState("");
    const [editedFile, setEditedFile] = useState("");

    const isEditing = currentMode !== 'view';

    useEffect(() => {
        listSkills().then(setSummaries);
    }, []);

    // use selectedSummary to switch to viewing/editing mode immediately,
    // and wait for content fetching there
    useEffect(() => {
        if (!selectedSummary) return;

        setSelectedSkill(null);

        getSkill(selectedSummary.name)
            .then((skill) => { setSelectedSkill(skill); })
            .catch(() => { setSelectedSummary(null); });
    }, [selectedSummary]);

    const toCard = (skillSummary: SkillSummary) => (
        <SkillCard
            key={skillSummary.name}
            skillSummary={skillSummary}
            onClick={() => setSelectedSummary(skillSummary)}
        />
    );

    function handleBack() {
        handleCancel();
        setSelectedSummary(null);
    }

    function handleCreate() {
        setSelectedSummary(null);
        setSelectedSkill(null);
        setEditedName("");
        setEditedFile(newSkillTemplate);
        setCurrentMode('create');
    }

    async function handleUpload(event: ChangeEvent<HTMLInputElement>) {
        const file = event.target.files?.[0];
        if (!file) return;

        // fill the editor from an uploaded file
        setEditedFile(await file.text());
        setEditedName(file.name.replace(/\.(md|markdown)$/i, ""));

        // clear the input so picking the same file again still fires a change
        event.target.value = "";
    }

    function handleEdit() {
        if (!selectedSkill) return;

        setEditedFile(selectedSkill.file);
        setCurrentMode('edit');
    }

    function handleCancel() {
        setCurrentMode('view');
    }

    async function handleDelete() {
        if (!selectedSummary) return;

        try {
            await deleteSkill(selectedSummary.name);
        } catch {
            // stay in edit mode if delete fails
            return;
        }

        // go back to the list
        setSummaries(await listSkills());
        setSelectedSummary(null);
        setSelectedSkill(null);
        setCurrentMode('view');
    }

    // save handles both edit and create
    async function handleSave() {
        const name = currentMode === 'create' ? editedName.trim() : selectedSummary!.name;

        try {
            if (currentMode === 'create') {
                await createSkill(name, editedFile);
            } else {
                await updateSkill(name, editedFile);
            }
        } catch {
            // stay in edit mode so the draft survives
            return;
        }

        // Refresh via refetch
        const refreshed = await listSkills();
        setSummaries(refreshed);
        setSelectedSummary(refreshed.find((summary) => summary.name === name) ?? null);
        setCurrentMode('view');
    }

    function headerButtons() {
        switch (currentMode) {
            case 'view':
                return (
                    <>
                        <Button
                            size={isMobile ? "icon" : "lg"}
                            variant="outline"
                            onClick={() => selectedSkill && downloadTextFile(`${selectedSkill.name}.md`, selectedSkill.file, "text/markdown")}
                        ><DownloadIcon />{!isMobile && "Download"}</Button>
                        <Button
                            size={isMobile ? "icon" : "lg"}
                            onClick={handleEdit}
                        ><PencilIcon />{!isMobile && "Edit"}</Button>
                    </>
                );
            case 'edit':
                return (
                    <Dialog>
                        <DialogTrigger asChild>
                            <Button
                                size={isMobile ? "icon" : "lg"}
                                variant="destructive"
                            ><Trash2Icon />{!isMobile && "Delete Skill"}</Button>
                        </DialogTrigger>
                        <DialogContent>
                            <DialogHeader>
                                <DialogTitle>Delete {selectedSummary?.name}.md?</DialogTitle>
                            </DialogHeader>
                            <DialogFooter>
                                <DialogClose asChild>
                                    <Button variant="outline">Cancel</Button>
                                </DialogClose>
                                <DialogClose asChild>
                                    <Button variant="destructive" onClick={handleDelete}>Delete</Button>
                                </DialogClose>
                            </DialogFooter>
                        </DialogContent>
                    </Dialog>
                );
            case 'create':
                // hidden label & input to let button act as file upload
                return (
                    <Button size={isMobile ? "icon" : "lg"} asChild>
                        <label className="cursor-pointer">
                            <UploadIcon />{!isMobile && "Upload Skill"}
                            <input
                                type="file"
                                accept=".md,.markdown,text/markdown"
                                onChange={handleUpload}
                                className="hidden"
                            />
                        </label>
                    </Button>
                );
        }
    }

    return (
        <div>
            {(selectedSummary || currentMode === 'create') ? (
                <div className={cn("flex flex-col gap-4", isMobile ? "" : "p-8")}>
                    {/** skill viewing/editing page */}
                    {/** header */}
                    <div className={cn("flex items-center justify-between", !isMobile && "pt-4")}>
                        <div className="flex items-center gap-2">
                            <Button variant="outline" size="icon" 
                                onClick={handleBack}
                            ><ArrowLeft /></Button>
                            <h2 className="text-xl font-semibold">{currentMode === 'create' ? "New Skill": selectedSummary!.name}</h2>
                        </div>
                        
                        <div className="flex items-center gap-2">
                            {headerButtons()}
                        </div>
                        
                    </div>
                    
                    {/** view mode only summary */}
                    {!isEditing && <p className="px-1">{selectedSummary!.summary}</p>}

                    <div className="flex flex-col gap-4 max-w-3xl w-full mx-auto">
                        {/** create mode only name input */}
                        {currentMode === 'create' &&
                            <div className="flex items-center gap-1">
                                <Input
                                    value={editedName}
                                    onChange={(event) => setEditedName(event.target.value)}
                                    placeholder="skill_name"
                                    className="text-xl font-semibold max-w-2xs"
                                />
                                <span className="text-xl font-semibold">.md</span>
                            </div>
                        }

                        {/** markdown renderer/text editor */}
                        {isEditing
                            ? <Textarea
                                value={editedFile}
                                onChange={(event) => setEditedFile(event.target.value)}
                                spellCheck={false}
                                className={cn(containerStyle, "resize-none font-mono text-sm focus-visible:ring-0")}
                            />
                            : selectedSkill && (
                                <div className={cn(markdownStyle, containerStyle)}>
                                    <Markdown remarkPlugins={[remarkGfm, remarkFrontmatter]} components={markdownComponents}>
                                        {selectedSkill.file}
                                    </Markdown>
                                </div>
                            )
                        }

                        {/** edit mode controls */}
                        {isEditing &&
                            <div className="flex justify-end items-center gap-2">
                                <Button size="lg" variant="outline" onClick={handleCancel}>Cancel</Button>
                                <Button
                                    size="lg"
                                    disabled={currentMode === 'create' && !editedName.trim()}
                                    onClick={handleSave}
                                >Confirm</Button>
                            </div>
                        }
                    </div>
                </div>
            ) : (
                <div className={isMobile ? "" : "p-8"}>
                    {/** skill cards grid/list */}
                    {/** header */}
                    { !isMobile && <h1 className="pb-4">{title}</h1> }
                    <span className="pl-1 text-muted-foreground">{summaries.length} skills</span>
                    {/** desktop grid, mobile rows */}
                    <div className={isMobile
                        ? "py-4 flex flex-col gap-2"
                        : "py-4 grid grid-cols-[repeat(auto-fill,_16rem)] gap-4"}>
                        {summaries.map(toCard)}
                        <NewSkillCard onClick={handleCreate} />
                    </div>
                </div>
            )}
        </div>
    )
}