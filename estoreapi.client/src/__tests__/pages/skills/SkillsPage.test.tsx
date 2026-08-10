import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, it, expect } from 'vitest'
import SkillsPage from '@/pages/skills/SkillsPage'
import { skillFileFixtures } from '../../mocks/fixtures'

// MSW fixture data: intake_job and order_parts, each with a summary and a markdown file

// The new-skill card and the back button are icon-only, so they carry no text.
// They are never on screen together, which makes "the button with no text" unambiguous.
const iconOnlyButton = () => screen.getAllByRole('button').find((button) => button.textContent === '')!

// Both the name Input and the file Textarea expose role 'textbox', so tests that
// care about the editor specifically have to filter by tag.
const textareas = () => screen.queryAllByRole('textbox').filter((box) => box.tagName === 'TEXTAREA')

/** Open intake_job and wait for its file to load. */
async function openSkill() {
    await userEvent.click(await screen.findByRole('button', { name: /intake_job/i }))
    // The file arrives after a second request, so wait for its rendered content
    await screen.findByText('Book a warranty repair.')
}

describe('SkillsPage', () => {
    it('renders every skill from the API as a card', async () => {
        render(<SkillsPage title="Skills" />)

        expect(await screen.findByText('intake_job')).toBeInTheDocument()
        expect(screen.getByText('Books a warranty repair job for an existing customer.')).toBeInTheDocument()
        expect(screen.getByText('order_parts')).toBeInTheDocument()
        expect(screen.getByText('Orders replacement parts for a diagnosed device.')).toBeInTheDocument()
        expect(screen.getByText('2 skills')).toBeInTheDocument()
    })

    it('clicking a card hides the cards and opens the skill in view mode', async () => {
        render(<SkillsPage title="Skills" />)
        await openSkill()

        // The other card is gone, so the grid has been replaced by the panel
        expect(screen.queryByText('order_parts')).not.toBeInTheDocument()

        // View mode renders the file as markdown, not as an editor
        expect(screen.getByRole('heading', { name: 'Goal' })).toBeInTheDocument()
        expect(textareas()).toHaveLength(0)

        // and offers the view-mode actions
        expect(screen.getByRole('button', { name: /download/i })).toBeInTheDocument()
        expect(screen.getByRole('button', { name: /edit/i })).toBeInTheDocument()
    })

    it('clicking the new card opens create mode with the inputs and the empty template', async () => {
        render(<SkillsPage title="Skills" />)
        await screen.findByText('intake_job')

        await userEvent.click(iconOnlyButton())

        expect(screen.getByText('New Skill')).toBeInTheDocument()
        // Create mode is the only mode with a name input alongside the editor
        expect(screen.getByPlaceholderText('skill_name')).toBeInTheDocument()

        const [editor] = textareas()
        expect(editor).toHaveValue(
            '---\nsummary: One sentence on what this skill does and when to use it.\n---\n\nContent here...\n'
        )

        // The upload control is a label wrapping a hidden file input, so it has no button role
        expect(screen.getByText('Upload Skill')).toBeInTheDocument()
    })

    it('clicking edit in view mode switches to edit mode with one textarea only', async () => {
        render(<SkillsPage title="Skills" />)
        await openSkill()

        await userEvent.click(screen.getByRole('button', { name: /edit/i }))

        // The editor is the only textbox: no name input, unlike create mode
        expect(screen.getAllByRole('textbox')).toHaveLength(1)
        const [editor] = textareas()
        expect(editor).toHaveValue(skillFileFixtures.intake_job)

        // The rendered markdown is replaced by the raw file
        expect(screen.queryByRole('heading', { name: 'Goal' })).not.toBeInTheDocument()
        expect(screen.getByRole('button', { name: 'Confirm' })).toBeInTheDocument()
        expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument()
    })

    it('the back button returns to the cards from view mode', async () => {
        render(<SkillsPage title="Skills" />)
        await openSkill()

        await userEvent.click(iconOnlyButton())

        expect(await screen.findByText('order_parts')).toBeInTheDocument()
        expect(screen.getByText('2 skills')).toBeInTheDocument()
    })

    it('the back button returns to the cards from edit mode, and reopening defaults to view mode', async () => {
        render(<SkillsPage title="Skills" />)
        await openSkill()
        await userEvent.click(screen.getByRole('button', { name: /edit/i }))

        await userEvent.click(iconOnlyButton())

        expect(await screen.findByText('order_parts')).toBeInTheDocument()

        // Reopening starts in view mode rather than resuming the edit
        await openSkill()
        expect(screen.getByRole('button', { name: /edit/i })).toBeInTheDocument()
        expect(textareas()).toHaveLength(0)
    })

    it('the delete button opens a confirmation dialog naming the skill', async () => {
        render(<SkillsPage title="Skills" />)
        await openSkill()
        await userEvent.click(screen.getByRole('button', { name: /edit/i }))

        await userEvent.click(screen.getByRole('button', { name: /delete skill/i }))

        // Deleting is confirmed in a dialog rather than happening on the first click
        const dialog = await screen.findByRole('dialog')
        expect(within(dialog).getByText('Delete intake_job.md?')).toBeInTheDocument()
        expect(within(dialog).getByRole('button', { name: 'Delete' })).toBeInTheDocument()
        expect(within(dialog).getByRole('button', { name: 'Cancel' })).toBeInTheDocument()
    })

    it('cancelling an edit returns to view mode', async () => {
        render(<SkillsPage title="Skills" />)
        await openSkill()
        await userEvent.click(screen.getByRole('button', { name: /edit/i }))

        await userEvent.click(screen.getByRole('button', { name: 'Cancel' }))

        // The editor is replaced by the rendered file again, still on the same skill
        expect(textareas()).toHaveLength(0)
        expect(screen.getByRole('heading', { name: 'Goal' })).toBeInTheDocument()
        expect(screen.getByRole('button', { name: /edit/i })).toBeInTheDocument()
    })

    it('cancelling a new skill returns to the cards', async () => {
        render(<SkillsPage title="Skills" />)
        await screen.findByText('intake_job')
        await userEvent.click(iconOnlyButton())

        await userEvent.click(screen.getByRole('button', { name: 'Cancel' }))

        // A new skill has no saved file to fall back to, so cancelling leaves the panel entirely
        expect(await screen.findByText('order_parts')).toBeInTheDocument()
        expect(screen.getByText('2 skills')).toBeInTheDocument()
    })

    it('the back button returns to the cards from create mode, and the new card reopens it empty', async () => {
        render(<SkillsPage title="Skills" />)
        await screen.findByText('intake_job')
        await userEvent.click(iconOnlyButton())

        // Type into the draft so a resumed session would be visible
        await userEvent.type(screen.getByPlaceholderText('skill_name'), 'draft_skill')
        await userEvent.click(iconOnlyButton())

        expect(await screen.findByText('order_parts')).toBeInTheDocument()

        await userEvent.click(iconOnlyButton())
        expect(screen.getByPlaceholderText('skill_name')).toHaveValue('')
    })
})
