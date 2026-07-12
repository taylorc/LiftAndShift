<script setup lang="ts">
import { Plus, Settings, MoreHorizontal } from '@lucide/vue'
import { MiddlewareList, MiddlewareName } from '~/lib/middlewareList'
import {
  TodoListDto,
  TodoItemDto,
  CreateTodoListCommand,
  CreateTodoItemCommand,
  UpdateTodoItemCommand,
  UpdateTodoItemDetailCommand,
  UpdateTodoListCommand,
  LookupDto,
  ColourDto,
} from '~/lib/web-api-client'

const list = new MiddlewareList();
definePageMeta({ middleware: list.getStringValueForEmum(MiddlewareName.Protected) })

const listsClient = useTodoListsClient()
const itemsClient = useTodoItemsClient()

// ── Data state ────────────────────────────────────────────────────────────────
const lists = ref<TodoListDto[] | null>(null)
const priorityLevels = ref<LookupDto[]>([])
const colours = ref<ColourDto[]>([])
const selectedListId = ref<number | null>(null)

// ── New list dialog state ──────────────────────────────────────────────────────
const newListTitle = ref('')
const newListColour = ref('')
const newListError = ref('')

// ── List options dialog state ──────────────────────────────────────────────────
const listOptionsEditor = ref<{ id?: number; title?: string; colour?: string }>({})

// ── Item state ────────────────────────────────────────────────────────────────
const selectedItem = ref<TodoItemDto | null>(null)
const editingItem = ref<TodoItemDto | null>(null)
const editValue = ref('')
const newItemTitle = ref('')
const addingItem = ref(false)
const itemDetailsEditor = ref<{ listId?: number; priority?: number; note?: string }>({})

// ── Mutable flags (cancel detection on blur) ──────────────────────────────────
const originalTitle = ref('')
const editCancelled = ref(false)
const newItemCancelled = ref(false)

// ── Dialog template refs ──────────────────────────────────────────────────────
const newListDialogRef = ref<HTMLDialogElement | null>(null)
const listOptionsDialogRef = ref<HTMLDialogElement | null>(null)
const deleteListDialogRef = ref<HTMLDialogElement | null>(null)
const itemDetailsDialogRef = ref<HTMLDialogElement | null>(null)

// ── Load data ─────────────────────────────────────────────────────────────────
onMounted(async () => {
  try {
    const result = await listsClient.getTodoLists()
    lists.value = result.lists ?? []
    priorityLevels.value = result.priorityLevels ?? []
    colours.value = result.colours ?? []
    newListColour.value = result.colours?.[0]?.code ?? ''
    if (result.lists?.length) selectedListId.value = result.lists[0].id ?? null
  } catch (e) { console.error(e) }
})

watch(selectedListId, () => {
  newItemTitle.value = ''
  addingItem.value = false
})

// ── Computed ──────────────────────────────────────────────────────────────────
const selectedList = computed(() => lists.value?.find(l => l.id === selectedListId.value) ?? null)
const remainingItems = (list: TodoListDto) => list.items?.filter(t => !t.done).length ?? 0

// ── List actions ──────────────────────────────────────────────────────────────
function showNewListDialog() {
  newListTitle.value = ''
  newListColour.value = colours.value[0]?.code ?? ''
  newListError.value = ''
  newListDialogRef.value?.showModal()
  setTimeout(() => document.getElementById('newListTitle')?.focus(), 50)
}

function closeNewListDialog() {
  newListDialogRef.value?.close()
  newListTitle.value = ''
  newListColour.value = colours.value[0]?.code ?? ''
  newListError.value = ''
}

async function commitNewList() {
  if (!newListTitle.value.trim()) return
  try {
    const id = await listsClient.createTodoList(new CreateTodoListCommand({ title: newListTitle.value.trim(), colour: newListColour.value }))
    const newList = new TodoListDto({ id, title: newListTitle.value.trim(), colour: newListColour.value, items: [] })
    lists.value = [...(lists.value ?? []), newList]
    selectedListId.value = id ?? null
    closeNewListDialog()
  } catch (e: any) {
    try {
      const errors = JSON.parse(e.response).errors
      if (errors?.Title) { newListError.value = errors.Title[0]; return }
    } catch { /* ignore */ }
    newListError.value = 'Failed to create list.'
  }
}

function showListOptionsDialog() {
  if (!selectedList.value) return
  Object.assign(listOptionsEditor, {
    id: selectedList.value.id,
    title: selectedList.value.title,
    colour: selectedList.value.colour || colours.value[0]?.code,
  })
  listOptionsDialogRef.value?.showModal()
}

function closeListOptionsDialog() {
  listOptionsDialogRef.value?.close()
  Object.assign(listOptionsEditor, { id: undefined, title: undefined, colour: undefined })
}

async function updateListOptions() {
  if (!selectedList.value) return
  try {
    await listsClient.updateTodoList(selectedList.value.id!, new UpdateTodoListCommand({
      id: listOptionsEditor.value.id,
      title: listOptionsEditor.value.title,
      colour: listOptionsEditor.value.colour,
    }))
    lists.value = lists.value!.map((l: TodoListDto) =>
      l.id === selectedList.value!.id
        ? new TodoListDto({ ...l, title: listOptionsEditor.value.title, colour: listOptionsEditor.value.colour })
        : l,
    )
    closeListOptionsDialog()
  } catch (e) { console.error(e) }
}

function confirmDeleteList() {
  closeListOptionsDialog()
  deleteListDialogRef.value?.showModal()
}

function closeDeleteListDialog() {
  deleteListDialogRef.value?.close()
}

async function deleteListConfirmed() {
  if (!selectedList.value) return
  try {
    await listsClient.deleteTodoList(selectedList.value.id!)
    const remaining = lists.value!.filter((l: TodoListDto) => l.id !== selectedList.value!.id)
    lists.value = remaining
    selectedListId.value = remaining.length ? (remaining[0].id ?? null) : null
    closeDeleteListDialog()
  } catch (e) { console.error(e) }
}

// ── Item actions ──────────────────────────────────────────────────────────────
function showItemDetailsDialog(item: TodoItemDto) {
  selectedItem.value = item
  Object.assign(itemDetailsEditor, { listId: item.listId, priority: item.priority, note: item.note })
  itemDetailsDialogRef.value?.showModal()
}

function closeItemDetailsDialog() {
  itemDetailsDialogRef.value?.close()
  selectedItem.value = null
  Object.assign(itemDetailsEditor, { listId: undefined, priority: undefined, note: undefined })
}

async function updateItemDetails() {
  if (!selectedItem.value) return
  const isMoving = selectedItem.value.listId !== itemDetailsEditor.value.listId
  try {
    await itemsClient.updateTodoItemDetail(selectedItem.value.id!, new UpdateTodoItemDetailCommand({
      id: selectedItem.value.id,
      listId: itemDetailsEditor.value.listId,
      priority: itemDetailsEditor.value.priority,
      note: itemDetailsEditor.value.note,
    }))
    lists.value = lists.value!.map((l: TodoListDto) => {
      if (l.id === selectedItem.value!.listId && isMoving)
        return new TodoListDto({ ...l, items: l.items?.filter((i: TodoItemDto) => i.id !== selectedItem.value!.id) })
      if (l.id === itemDetailsEditor.value.listId && isMoving)
        return new TodoListDto({ ...l, items: [...(l.items ?? []), new TodoItemDto({ ...selectedItem.value!, ...itemDetailsEditor })] })
      if (l.id === selectedItem.value!.listId)
        return new TodoListDto({ ...l, items: l.items?.map((i: TodoItemDto) => i.id === selectedItem.value!.id ? new TodoItemDto({ ...i, priority: itemDetailsEditor.value.priority, note: itemDetailsEditor.value.note }) : i) })
      return l
    })
    closeItemDetailsDialog()
  } catch (e) { console.error(e) }
}

async function deleteItem(item: TodoItemDto) {
  if (itemDetailsDialogRef.value?.open) closeItemDetailsDialog()
  try {
    await itemsClient.deleteTodoItem(item.id!)
    lists.value = lists.value!.map((l: TodoListDto) =>
      l.id === selectedListId.value
        ? new TodoListDto({ ...l, items: l.items?.filter((i: TodoItemDto) => i.id !== item.id) })
        : l,
    )
  } catch (e) { console.error(e) }
}

async function updateCheckbox(item: TodoItemDto, done: boolean) {
  const updated = new TodoItemDto({ ...item, done })
  lists.value = lists.value!.map((l: TodoListDto) =>
    l.id === selectedListId.value
      ? new TodoListDto({ ...l, items: l.items?.map((i: TodoItemDto) => i.id === item.id ? updated : i) })
      : l,
  )
  try { await itemsClient.updateTodoItem(item.id!, new UpdateTodoItemCommand({ id: item.id, title: item.title, done })) }
  catch (e) { console.error(e) }
}

function editItem(item: TodoItemDto, inputId: string) {
  originalTitle.value = item.title ?? ''
  editValue.value = item.title ?? ''
  editingItem.value = item
  setTimeout(() => document.getElementById(inputId)?.focus(), 100)
}

function cancelEdit(e?: Event) {
  editCancelled.value = true
  lists.value = lists.value!.map((l: TodoListDto) => new TodoListDto({
    ...l,
    items: l.items?.map((i: TodoItemDto) => i === editingItem.value ? new TodoItemDto({ ...i, title: originalTitle.value }) : i),
  }))
  editingItem.value = null
  ;(e?.target as HTMLElement)?.blur()
}

async function commitEdit() {
  if (!editValue.value.trim()) {
    await deleteItem(editingItem.value!)
    editingItem.value = null
    return
  }
  const updated = new TodoItemDto({ ...editingItem.value!, title: editValue.value.trim() })
  lists.value = lists.value!.map((l: TodoListDto) =>
    l.id === selectedListId.value
      ? new TodoListDto({ ...l, items: l.items?.map((i: TodoItemDto) => i === editingItem.value ? updated : i) })
      : l,
  )
  const prev = editingItem.value!
  editingItem.value = null
  try { await itemsClient.updateTodoItem(updated.id!, new UpdateTodoItemCommand({ id: updated.id, title: updated.title, done: updated.done })) }
  catch (e) { console.error(e) }
}

function startAddingItem() {
  addingItem.value = true
  setTimeout(() => document.getElementById('newItemInput')?.focus(), 50)
}

function cancelNewItem(e?: Event) {
  newItemCancelled.value = true
  addingItem.value = false
  newItemTitle.value = ''
  ;(e?.target as HTMLElement)?.blur()
}

async function commitNewItem() {
  addingItem.value = false
  if (!newItemTitle.value.trim()) { newItemTitle.value = ''; return }
  const title = newItemTitle.value.trim()
  const listId = selectedListId.value!
  newItemTitle.value = ''
  try {
    const id = await itemsClient.createTodoItem(new CreateTodoItemCommand({ title, listId }))
    lists.value = lists.value!.map((l: TodoListDto) =>
      l.id === listId
        ? new TodoListDto({ ...l, items: [...(l.items ?? []), new TodoItemDto({ id, listId, title, done: false, priority: priorityLevels.value[0]?.id })] })
        : l,
    )
  } catch (e) { console.error(e) }
}
</script>

<template>
  <span v-if="!lists" aria-busy="true">Loading&hellip;</span>

  <template v-else>
    <hgroup>
      <h1>Tasks</h1>
      <p>Manage your todo lists and tasks.</p>
    </hgroup>

    <div class="todo-layout">

      <!-- Sidebar -->
      <div class="todo-sidebar">
        <div class="todo-panel-header">
          <h2>Lists</h2>
          <button class="icon-btn" @click="showNewListDialog"><Plus :size="20" :stroke-width="2" /></button>
        </div>
        <ul>
          <li
            v-for="list in lists"
            :key="list.id"
            :aria-current="selectedList === list ? 'true' : undefined"
            @click="selectedListId = list.id ?? null"
          >
            <span class="colour-dot" :style="{ background: list.colour }" aria-hidden="true"></span>
            <span>{{ list.title }}</span>
            <small>{{ remainingItems(list) }}</small>
          </li>
        </ul>
      </div>

      <!-- Items panel -->
      <div v-if="selectedList" class="todo-main">
        <div class="todo-panel-header">
          <h2 :style="{ color: selectedList.colour }">{{ selectedList.title }}</h2>
          <button class="icon-btn" @click="showListOptionsDialog"><Settings :size="20" :stroke-width="2" /></button>
        </div>

        <div
          v-for="(item, i) in selectedList.items"
          :key="item.id"
          class="todo-item"
        >
          <input type="checkbox" :checked="item.done" @change="updateCheckbox(item, ($event.target as HTMLInputElement).checked)" />
          <input
            v-if="editingItem === item"
            :id="`itemTitle${i}`"
            v-model="editValue"
            type="text"
            class="todo-item-input"
            autofocus
            maxlength="200"
            @keydown.enter="($event.target as HTMLElement).blur()"
            @keydown.escape="cancelEdit($event)"
            @blur="() => { if (editCancelled) { editCancelled = false; return } commitEdit() }"
          />
          <span
            v-else
            :class="`todo-item-text${item.done ? ' todo-done' : ''}`"
            @click="editItem(item, `itemTitle${i}`)"
          >{{ item.title }}</span>
          <button v-if="item.id !== 0" class="icon-btn" @click="showItemDetailsDialog(item)">
            <MoreHorizontal :size="20" :stroke-width="2" />
          </button>
        </div>

        <div class="todo-item todo-new-item">
          <input type="checkbox" disabled />
          <input
            v-if="addingItem"
            id="newItemInput"
            v-model="newItemTitle"
            type="text"
            class="todo-item-input"
            maxlength="200"
            @keydown.enter="commitNewItem"
            @keydown.escape="cancelNewItem($event)"
            @blur="() => { if (newItemCancelled) { newItemCancelled = false; return } commitNewItem() }"
          />
          <span
            v-else
            class="todo-item-text todo-new-item-placeholder"
            @click="startAddingItem"
          >New task…</span>
        </div>
      </div>
    </div>

    <!-- New List dialog -->
    <dialog ref="newListDialogRef">
      <article>
        <header>
          <h3>New List</h3>
          <button rel="prev" aria-label="Close" @click="closeNewListDialog"></button>
        </header>
        <label for="newListTitle">Title</label>
        <input
          id="newListTitle"
          v-model="newListTitle"
          type="text"
          placeholder="List title…"
          :aria-invalid="newListError ? 'true' : undefined"
          maxlength="200"
          @keydown.enter="commitNewList"
        />
        <small v-if="newListError">{{ newListError }}</small>
        <label>Colour</label>
        <div class="colour-picker">
          <button
            v-for="c in colours"
            :key="c.code"
            type="button"
            :class="`colour-swatch${newListColour === c.code ? ' selected' : ''}`"
            :style="{ background: c.code }"
            :aria-label="c.name"
            @click="newListColour = c.code ?? ''"
          />
        </div>
        <footer>
          <button class="secondary" @click="closeNewListDialog">Cancel</button>
          <button @click="commitNewList">Create</button>
        </footer>
      </article>
    </dialog>

    <!-- List Options dialog -->
    <dialog ref="listOptionsDialogRef">
      <article>
        <header>
          <h3>List Options</h3>
          <button rel="prev" aria-label="Close" @click="closeListOptionsDialog"></button>
        </header>
        <label for="listOptionsTitle">Title</label>
        <input
          id="listOptionsTitle"
          v-model="listOptionsEditor.title"
          type="text"
          placeholder="List name…"
          maxlength="200"
          @keydown.enter="updateListOptions"
        />
        <label>Colour</label>
        <div class="colour-picker">
          <button
            v-for="c in colours"
            :key="c.code"
            type="button"
            :class="`colour-swatch${listOptionsEditor.colour === c.code ? ' selected' : ''}`"
            :style="{ background: c.code }"
            :aria-label="c.name"
            @click="listOptionsEditor.colour = c.code ?? ''"
          />
        </div>
        <footer>
          <button class="danger" style="margin-inline-end: auto" @click="confirmDeleteList">Delete</button>
          <button class="secondary" @click="closeListOptionsDialog">Cancel</button>
          <button @click="updateListOptions">Update</button>
        </footer>
      </article>
    </dialog>

    <!-- Delete List dialog -->
    <dialog ref="deleteListDialogRef">
      <article>
        <header>
          <h3>Delete "{{ selectedList?.title }}"?</h3>
          <button rel="prev" aria-label="Close" @click="closeDeleteListDialog"></button>
        </header>
        <p>All items will be permanently deleted.</p>
        <footer>
          <button class="secondary" @click="closeDeleteListDialog">Cancel</button>
          <button
            class="danger"
            style="--pico-background-color: var(--pico-del-color); --pico-border-color: var(--pico-del-color); --pico-color: #fff"
            @click="deleteListConfirmed"
          >Delete</button>
        </footer>
      </article>
    </dialog>

    <!-- Item Details dialog -->
    <dialog ref="itemDetailsDialogRef">
      <article>
        <header>
          <h3>Item Details</h3>
          <button rel="prev" aria-label="Close" @click="closeItemDetailsDialog"></button>
        </header>
        <label for="itemList">List</label>
        <select
          id="itemList"
          :value="itemDetailsEditor.listId"
          @change="itemDetailsEditor.listId = +($event.target as HTMLSelectElement).value"
        >
          <option v-for="list in lists" :key="list.id" :value="list.id">{{ list.title }}</option>
        </select>
        <label for="itemPriority">Priority</label>
        <select
          id="itemPriority"
          :value="itemDetailsEditor.priority"
          @change="itemDetailsEditor.priority = +($event.target as HTMLSelectElement).value"
        >
          <option v-for="level in priorityLevels" :key="level.id" :value="level.id">{{ level.title }}</option>
        </select>
        <label for="itemNote">Note</label>
        <textarea id="itemNote" v-model="itemDetailsEditor.note" rows="3"></textarea>
        <footer>
          <button class="danger" style="margin-inline-end: auto" @click="deleteItem(selectedItem!)">Delete</button>
          <button class="secondary" @click="closeItemDetailsDialog">Cancel</button>
          <button @click="updateItemDetails">Update</button>
        </footer>
      </article>
    </dialog>
  </template>
</template>
