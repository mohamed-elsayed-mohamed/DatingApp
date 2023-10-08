import { User } from './user';

export class UserParams {
  gender: string;
  minAge: number = 10;
  maxAge: number = 99;
  pageNumber: number = 1;
  pageSize: number = 3;
  orderBy: string = 'lastActive';

  constructor(user: User) {
    this.gender = user.gender === 'female' ? 'male' : 'female';
  }
}
