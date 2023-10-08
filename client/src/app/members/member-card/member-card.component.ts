import { MembersService } from 'src/app/services/members.service';
import { Component, Input, ViewEncapsulation } from '@angular/core';
import { Member } from 'src/app/models/member';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css'],
  encapsulation: ViewEncapsulation.Emulated,
})
export class MemberCardComponent {
  @Input() member: Member | undefined;

  constructor(
    private membersService: MembersService,
    private toastr: ToastrService
  ) {}

  addLike(member: Member) {
    this.membersService.addLike(member.userName).subscribe({
      next: () => {
        this.toastr.success('You have liked' + member.knownAs);
      },
    });
  }
}
