import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CopilotChatComponent } from './shared/components/copilot-chat/copilot-chat.component';
import { NavShellComponent } from './shared/components/nav-shell/nav-shell.component';
import { ToastComponent } from './shared/components/toast/toast.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NavShellComponent, ToastComponent, CopilotChatComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {}
